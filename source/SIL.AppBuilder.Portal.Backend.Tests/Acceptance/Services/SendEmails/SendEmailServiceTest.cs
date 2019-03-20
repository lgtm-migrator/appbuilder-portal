﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OptimaJet.DWKit.StarterApplication.Data;
using OptimaJet.DWKit.StarterApplication.Models;
using OptimaJet.DWKit.StarterApplication.Services;
using SIL.AppBuilder.Portal.Backend.Tests.Acceptance.Support;
using SIL.AppBuilder.Portal.Backend.Tests.Support.StartupScenarios;
using Xunit;

namespace SIL.AppBuilder.Portal.Backend.Tests.Acceptance.Services.SendEmails
{
    [Collection("BuildEngineCollection")]
    public class SendEmailServiceTest : BaseTest<BuildEngineStartup>
    {
        public User CurrentUser { get; set; }
        public Notification notification1 { get; set; }
        public Notification notification2 { get; set; }
        public User user1 { get; private set; }
        public User user2 { get; set; }
        public Group group1 { get; set; }
        public Organization org1 { get; private set; }
        public Organization org2 { get; private set; }
        public OrganizationMembership orgmember1 { get; set; }
        public OrganizationMembership orgmember2 { get; set; }
        public OrganizationMembership orgmember3 { get; set; }
        public UserRole ur1 { get; set; }
        public UserRole ur2 { get; set; }
        public UserRole ur3 { get; set; }
        public UserRole ur4 { get; set; }
        public Project project1 { get; set; }
        public ApplicationType type1 { get; set; }
        public Product product1 { get; set; }
        public ProductDefinition productDefinition1 { get; set; }
        public ProductArtifact productArtifact1 { get; set; }
        public ProductArtifact productArtifact2 { get; set; }
        public ProductArtifact productArtifact3 { get; set; }
        public ProductBuild productBuild1 { get; set; }
        public Reviewer reviewer1 { get; set; }
        public Reviewer reviewer2 { get; set; }
        public WorkflowDefinition workflow1 { get; set; }
        private void BuildTestData()
        {
            var tuple = NeedsConfiguredCurrentUser();
            CurrentUser = tuple.Item1;
            orgmember1 = tuple.Item2;
            org1 = tuple.Item3;

            var roleSA = AddEntity<AppDbContext, Role>(new Role { RoleName = RoleName.SuperAdmin });
            var roleOA = AddEntity<AppDbContext, Role>(new Role { RoleName = RoleName.OrganizationAdmin });
            var roleAB = AddEntity<AppDbContext, Role>(new Role { RoleName = RoleName.AppBuilder });

            user1 = AddEntity<AppDbContext, User>(new User
            {
                ExternalId = "test-auth0-id1",
                Email = "test-email1@test.test",
                Name = "Test Testenson1",
                GivenName = "Test1",
                FamilyName = "Testenson1",
                Locale = "en-US",
                EmailNotification = true
            });
            user2 = AddEntity<AppDbContext, User>(new User
            {
                ExternalId = "test-auth0-id2",
                Email = "test-email1@test.test",
                Name = "Test Testenson2",
                GivenName = "Test1",
                FamilyName = "Testenson2",
                Locale = "en-US",
                EmailNotification = false
            });
            group1 = AddEntity<AppDbContext, Group>(new Group
            {
                Name = "TestGroup1",
                Abbreviation = "TG1",
                OwnerId = org1.Id
            });
            org2 = AddEntity<AppDbContext, Organization>(new Organization
            {
                OwnerId = CurrentUser.Id,
                Name = "Kalaam Media"
            });
            orgmember2 = AddEntity<AppDbContext, OrganizationMembership>(new OrganizationMembership
            {
                UserId = user1.Id,
                OrganizationId = org1.Id
            });
            orgmember3 = AddEntity<AppDbContext, OrganizationMembership>(new OrganizationMembership
            {
                UserId = user1.Id,
                OrganizationId = org2.Id
            });
            ur1 = AddEntity<AppDbContext, UserRole>(new UserRole
            {
                UserId = CurrentUser.Id,
                OrganizationId = org1.Id,
                RoleId = roleSA.Id
            });
            ur2 = AddEntity<AppDbContext, UserRole>(new UserRole
            {
                UserId = user1.Id,
                OrganizationId = org1.Id,
                RoleId = roleOA.Id
            });
            ur3 = AddEntity<AppDbContext, UserRole>(new UserRole
            {
                UserId = CurrentUser.Id,
                OrganizationId = org2.Id,
                RoleId = roleSA.Id
            });
            ur4 = AddEntity<AppDbContext, UserRole>(new UserRole
            {
                UserId = user2.Id,
                OrganizationId = org2.Id,
                RoleId = roleOA.Id
            });

            var notificationParm = new
            {
                orgName = "SIL International",
                url = "http://gtis.guru.com:8443",
                token = "replace"
            };
            var serializedParm = JsonConvert.SerializeObject(notificationParm);
            notification1 = AddEntity<AppDbContext, Notification>(new Notification
            {
                MessageId = "buildengineConnected",
                MessageSubstitutionsJson = serializedParm,
                Message = "Build Engine for organization SIL International status change: connected",
                UserId = user1.Id,
                SendEmail = true
            });
            type1 = AddEntity<AppDbContext, ApplicationType>(new ApplicationType
            {
                Name = "scriptureappbuilder",
                Description = "Scripture App Builder"
            });
            project1 = AddEntity<AppDbContext, Project>(new Project
            {
                Name = "Test Project1",
                TypeId = type1.Id,
                Description = "Test Description",
                OwnerId = user1.Id,
                GroupId = group1.Id,
                OrganizationId = org1.Id,
                Language = "eng-US",
                IsPublic = true,
                WorkflowProjectUrl = "www.workflow.url"
            });
            workflow1 = AddEntity<AppDbContext, WorkflowDefinition>(new WorkflowDefinition
            {
                Name = "TestWorkFlow",
                Enabled = true,
                Description = "This is a test workflow",
                WorkflowScheme = "Don't know what this is"
            });
            productDefinition1 = AddEntity<AppDbContext, ProductDefinition>(new ProductDefinition
            {
                Name = "TestProd1",
                TypeId = type1.Id,
                Description = "This is a test product",
                WorkflowId = workflow1.Id
            });
            product1 = AddEntity<AppDbContext, Product>(new Product
            {
                ProjectId = project1.Id,
                ProductDefinitionId = productDefinition1.Id
            });
            productBuild1 = AddEntity<AppDbContext, ProductBuild>(new ProductBuild
            {
                ProductId = product1.Id,
                BuildId = 1,
                Version = "1"
            });
            productArtifact1 = AddEntity<AppDbContext, ProductArtifact>(new ProductArtifact
            {
                ProductId = product1.Id,
                ProductBuildId = productBuild1.Id,
                ArtifactType = "apk",
                Url = "http://www.test.com/testfile.apk",
                FileSize = 1000,
                ContentType = "test"
            });
            productArtifact2 = AddEntity<AppDbContext, ProductArtifact>(new ProductArtifact
            {
                ProductId = product1.Id,
                ProductBuildId = productBuild1.Id,
                ArtifactType = "play-listing",
                Url = "http://www.test.com/listing.txt",
                FileSize = 1000,
                ContentType = "test"
            });
            productArtifact3 = AddEntity<AppDbContext, ProductArtifact>(new ProductArtifact
            {
                ProductId = product1.Id,
                ProductBuildId = productBuild1.Id,
                ArtifactType = "about",
                Url = "http://www.test.com/about.txt",
                FileSize = 1000,
                ContentType = "test"
            });

            reviewer1 = AddEntity<AppDbContext, Reviewer>(new Reviewer
            {
                Name = "David Moore",
                Email = "david_moore1@sil.org",
                ProjectId = project1.Id
            });
            reviewer2 = AddEntity<AppDbContext, Reviewer>(new Reviewer
            {
                Name = "Chris Hubbard",
                Email = "chris_hubbard@sil.org",
                ProjectId = project1.Id
            });
        }
        public SendEmailServiceTest(TestFixture<BuildEngineStartup> fixture) : base(fixture)
        {
        }
        [Fact]
        public async Task Send_EmailAsync()
        {
            BuildTestData();
            var sendEmailService = _fixture.GetService<SendEmailService>();
            var notifications = ReadTestData<AppDbContext, Notification>();
            await sendEmailService.SendNotificationEmailAsync(notifications[0]);
            var modifiedNotifications = ReadTestData<AppDbContext, Notification>();
            Assert.Single(modifiedNotifications);
            var notification = modifiedNotifications[0];
            var emails = ReadTestData<AppDbContext, Email>();
            Assert.Single(emails);
            var email = emails[0];
            Assert.Equal("Scriptoria: SIL International Build Engine Connected", email.Subject);
            Assert.Equal("Notification.txt", email.ContentTemplate);
            Assert.Equal("{\"Message\":\"<p>Build Engine for organization SIL International status change: connected</p>\",\"BuildEngineUrlText\":\"\",\"LinkUrl\":null}", email.ContentModelJson);
        }
        [Fact]
        public async Task Send_EmailAsyncWithLink()
        {
            BuildTestData();
            var notificationParm = new
            {
                orgName = "SIL International",
                url = "http://gtis.guru.com:8443",
                token = "replace"
            };
            var serializedParm = JsonConvert.SerializeObject(notificationParm);
            notification2 = AddEntity<AppDbContext, Notification>(new Notification
            {
                MessageId = "buildengineConnected",
                MessageSubstitutionsJson = serializedParm,
                Message = "Build Engine for organization SIL International status change: connected",
                UserId = user1.Id,
                LinkUrl = "https://buildengine.gtis.guru:8443"
            });
            var sendEmailService = _fixture.GetService<SendEmailService>();
            var notifications = ReadTestData<AppDbContext, Notification>();
            await sendEmailService.SendNotificationEmailAsync(notifications[1]);
            var modifiedNotifications = ReadTestData<AppDbContext, Notification>();
            Assert.Equal(2, modifiedNotifications.Count);
            var notification = modifiedNotifications[1];
            var emails = ReadTestData<AppDbContext, Email>();
            Assert.Single(emails);
            var email = emails[0];
            Assert.Equal("Scriptoria: SIL International Build Engine Connected", email.Subject);
            Assert.Equal("NotificationWithLink.txt", email.ContentTemplate);
            Assert.Equal("{\"Message\":\"<p>Build Engine for organization SIL International status change: connected</p>\",\"BuildEngineUrlText\":\"notifications.body.buildEngineUrl\",\"LinkUrl\":\"https://buildengine.gtis.guru:8443\"}", email.ContentModelJson);
        }
        [Fact]
        public void SendProductReviewEmail()
        {
            BuildTestData();
            var sendEmailService = _fixture.GetService<SendEmailService>();
            var actionParm = "{\"types\" : [\"apk\", \"play-listing\"]}";
            var parmsDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(actionParm);
            sendEmailService.SendProductReviewEmail(product1.Id, parmsDict);
            var emails = ReadTestData<AppDbContext, Email>();
            Assert.Equal(2, emails.Count);
            var expectedContent = "{\"Message\":\"<p>Please review TestProd1 for project Test Project1.</p><p>The links below will connect you with the product files to be reviewed:</p>\",\"Links\":\"<p><a href = http://www.test.com/testfile.apk>apk</a></p><p><a href = http://www.test.com/listing.txt>play-listing</a></p>\"}";
            Assert.Equal(expectedContent, emails[0].ContentModelJson);
            Assert.Equal("ReviewProduct.txt", emails[0].ContentTemplate);
            Assert.Equal("Scriptoria: TestProd1 Test Project1 Ready for Review", emails[0].Subject);
        }
    }
}
