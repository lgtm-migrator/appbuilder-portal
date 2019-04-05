﻿using System;
using System.Linq;
using JsonApiDotNetCore.Data;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;
using OptimaJet.DWKit.StarterApplication.Forms.Products;
using OptimaJet.DWKit.StarterApplication.Models;
using System.Threading.Tasks;
using JsonApiDotNetCore.Internal;
using OptimaJet.DWKit.StarterApplication.Repositories;
using System.Collections.Generic;
using static OptimaJet.DWKit.StarterApplication.Utility.ServiceExtensions;
using JsonApiDotNetCore.Internal.Query;
using Hangfire;
using OptimaJet.DWKit.StarterApplication.Services.Workflow;
using Microsoft.EntityFrameworkCore;

namespace OptimaJet.DWKit.StarterApplication.Services
{
    public class ProductService : EntityResourceService<Product, Guid>
    {
        IEntityRepository<Product, Guid> ProductRepository { get; set; }
        IEntityRepository<ProductDefinition> ProductDefinitionRepository { get; set; }
        public IEntityRepository<WorkflowDefinition> WorkflowDefinitionRepository { get; }
        IEntityRepository<Store> StoreRepository { get; }
        public IEntityRepository<UserRole> UserRolesRepository { get; }
        IBackgroundJobClient HangfireClient { get; }
        UserRepository UserRepository { get; set; }
        ProjectRepository ProjectRepository { get; set; }
        ICurrentUserContext CurrentUserContext { get; set; }

        IJsonApiContext JsonApiContext { get; }
        public IOrganizationContext OrganizationContext { get; private set; }

        public ProductService(
            IJsonApiContext jsonApiContext,
            IOrganizationContext organizationContext,
            IEntityRepository<Product, Guid> productRepository,
            UserRepository userRepository,
            ProjectRepository projectRepository,
            ICurrentUserContext currentUserContext,
            IEntityRepository<ProductDefinition> productDefinitionRepository,
            IEntityRepository<WorkflowDefinition> workflowDefinitionRepository,
            IEntityRepository<Store> storeRepository,
            IEntityRepository<UserRole> userRolesRepository,
            IBackgroundJobClient hangfireClient,
            ILoggerFactory loggerFactory) : base(jsonApiContext, productRepository, loggerFactory)
        {
            ProductRepository = productRepository;
            ProductDefinitionRepository = productDefinitionRepository;
            WorkflowDefinitionRepository = workflowDefinitionRepository;
            StoreRepository = storeRepository;
            UserRolesRepository = userRolesRepository;
            HangfireClient = hangfireClient;
            UserRepository = userRepository;
            ProjectRepository = projectRepository;
            CurrentUserContext = currentUserContext;
            OrganizationContext = organizationContext;
            JsonApiContext = jsonApiContext;

        }

        public override async Task<IEnumerable<Product>> GetAsync()
        {
            return await GetScopedToOrganization<Product>(base.GetAsync,
                                               OrganizationContext,
                                               JsonApiContext);
        }
        public override async Task<Product> GetAsync(Guid id)
        {
            var products = await GetAsync();
            return products.SingleOrDefault(p => p.Id == id);
        }

        public override async Task<Product> UpdateAsync(Guid id, Product resource)
        {
            //If changing organization, validate the change
            var updateForm = new UpdateForm(UserRepository,
                                            ProductRepository,
                                            ProductDefinitionRepository,
                                            StoreRepository,
                                            UserRolesRepository,
                                            ProjectRepository,
                                            OrganizationContext,
                                            CurrentUserContext);
            if (!updateForm.IsValid(id, resource))
            {
                throw new JsonApiException(updateForm.Errors);
            }

            var result = await base.UpdateAsync(id, resource);

            // TODO: figure out why this throws a NullReferenceException
            // await ProjectRepository.UpdateAsync(result.ProjectId, result.Project);

            return result;
        }
        public override async Task<Product> CreateAsync(Product resource)
        {
            var createForm = new CreateForm(ProjectRepository,
                                            ProductDefinitionRepository,
                                            StoreRepository,
                                            UserRolesRepository,
                                            UserRepository,
                                            CurrentUserContext);
            if (!createForm.IsValid(resource))
            {
                throw new JsonApiException(createForm.Errors);
            }

            var product = await base.CreateAsync(resource);

            // TODO: figure out why this throws a NullReferenceException
            // await ProjectRepository.UpdateAsync(result.ProjectId, result.Project);

            if (product != null)
            {
                HangfireClient.Enqueue<WorkflowProductService>(service => service.ManageNewProduct(product.Id));
            }
            return product;
        }

        public override async Task<bool> DeleteAsync(Guid id)
        {
            var products = await GetAsync();
            var product = products.SingleOrDefault(p => p.Id == id);
            if (product != null)
            {
                HangfireClient.Enqueue<WorkflowProductService>(service => service.ManageDeletedProduct(product.Id));
            }

            return await base.DeleteAsync(id);
        }

        public async Task<List<string>> GetProductActionsAsync(Guid id)
        {
            Product product = await GetProductForActions(id);

            if (product == null)
            {
                return null;
            }

            if (product.ProductWorkflow == null)
            {
                // No running workflow.  
                
                if (!product.DatePublished.HasValue)
                {
                    // Product has not been published
                    return null;
                }

                //Provide actions that are defined
                var result = new List<string>();
                if (product.ProductDefinition.RebuildWorkflowId.HasValue)
                {
                    result.Add(WorkflowType.Rebuild.ToString());
                }
                if (product.ProductDefinition.RepublishWorkflowId.HasValue)
                {
                    result.Add(WorkflowType.Republish.ToString());
                }
                return result;
            }

            var wd = await GetExecutingWorkflowDefintion(product);

            if (wd == null)
            {
                return null;
            }

            if (wd.Type != WorkflowType.Startup)
            {
                // Running a action workflow.  Provide cancel action
                return new List<string> { "Cancel" };
            }

            // Running the startup workflow.  Return empty list
            return new List<string> { };
        }

        private async Task<WorkflowDefinition> GetExecutingWorkflowDefintion(Product product)
        {
            return await WorkflowDefinitionRepository.Get()
                .Where(w => w.WorkflowScheme == product.ProductWorkflow.Scheme.SchemeCode)
                .FirstOrDefaultAsync();
        }

        private async Task<Product> GetProductForActions(Guid id)
        {
            return await ProductRepository.Get()
                .Where(p => p.Id == id)
                .Include(p => p.ProductWorkflow)
                    .ThenInclude(pw => pw.Scheme)
                .Include(p => p.ProductDefinition)
                .FirstOrDefaultAsync();
        }

        public async Task<WorkflowDefinition> RunProductActionAsync(Guid id, string type)
        {
            var product = await GetProductForActions(id);
            if (product == null)
            {
                return null;
            }

            if (product.ProductWorkflow == null)
            {
                // No running workflow.  Start one.
                int? workflowDefinitionId = null;
                if (type == WorkflowType.Rebuild.ToString())
                {
                    workflowDefinitionId = product.ProductDefinition.RebuildWorkflowId;
                }
                else if (type == WorkflowType.Republish.ToString())
                {
                    workflowDefinitionId = product.ProductDefinition.RepublishWorkflowId;
                }
                else
                {
                    throw new Exception($"Invalid type '{type}'");
                }

                if (workflowDefinitionId.HasValue)
                {
                    HangfireClient.Enqueue<WorkflowProductService>(service => service.StartProductWorkflow(id, workflowDefinitionId.Value));
                    return await WorkflowDefinitionRepository.GetAsync(workflowDefinitionId.Value);
                }

                throw new Exception($"Type '{type}' does not have workflow defined");
            }

            // Handle special case for "Cancel" action
            if (type == "Cancel")
            {
                var wd = await GetExecutingWorkflowDefintion(product);
                if (wd == null)
                {
                    throw new Exception("Could not find workflow definition!");
                }

                if (wd.Type == WorkflowType.Startup)
                {
                    throw new Exception("Cannot cancel a startup workflow");
                }

                HangfireClient.Enqueue<WorkflowProductService>(service => service.StopProductWorkflow(id));
                return wd;
            }

            // Trying to start an action workflow while one is already running
            throw new Exception("Cannot start a workflow while one is running");
        }
    }
}
