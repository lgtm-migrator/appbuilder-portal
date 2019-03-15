using System;
using Xunit;

namespace SIL.AppBuilder.BuildEngineApiClient.Tests.Integration
{
    public class ProjectTests
    {
        // Note: You should set these values to match your environment.  These
        // tests are not intended to be automated, just to interact with a real 
        // system and show how the API is used.
        //
        const string skipIntegrationTest = "Integration Test disabled"; // Set to null to be able to run/debug using Unit Test Runner
        public string BaseUrl { get; set; } = "https://buildengine.gtis.guru:8443"; // This is our staging version of BuildEngine
        public string ApiAccessKey { get; set; } = "";
        public string UserId { get; set; } = ""; // Email address
        public string GroupId { get; set; } = ""; // Some shared group

        [Theory(Skip = skipIntegrationTest)]
        [InlineData(4)]
        public void GetTestProject(int projectId)
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var response = client.GetProject(projectId);
            Assert.NotNull(response);
            Assert.NotEqual(0, response.Id);
        }

        [Fact(Skip = skipIntegrationTest)]
        public void GetTestProjects()
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var response = client.GetProjects();
            Assert.NotNull(response);
        }


        [Fact(Skip = skipIntegrationTest)]
        public void CreateTestProject()
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var project = new Project
            {
                UserId = this.UserId,
                GroupId = this.GroupId,
                AppId = "scriptureappbuilder",
                LanguageCode = "eng",
                ProjectName = Guid.NewGuid().ToString(),
            };

            var response = client.CreateProject(project);
            Assert.NotNull(response);
            Assert.NotEqual(0, response.Id);
            Assert.Equal(project.UserId, response.UserId);
            Assert.Equal(project.GroupId, response.GroupId);
            Assert.Equal(project.AppId, response.AppId);
            Assert.Equal(project.LanguageCode, response.LanguageCode);
            Assert.Equal("initialized", response.Status);
            Assert.Null(response.Result);
            Assert.Null(response.Error);
            Assert.Null(response.Url);
            Assert.NotEqual(DateTime.MinValue, response.Created);
            Assert.NotEqual(DateTime.MinValue, response.Updated);
        }
        [Fact(Skip = skipIntegrationTest)]
        public void CreateS3Project()
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var project = new Project
            {
                AppId = "scriptureappbuilder",
                LanguageCode = "eng",
                ProjectName = Guid.NewGuid().ToString(),
                StorageType = "s3"
            };
            var response = client.CreateProject(project);
            Assert.NotNull(response);
            Assert.NotEqual(0, response.Id);
            Assert.Equal(project.AppId, response.AppId);
            Assert.Equal(project.LanguageCode, response.LanguageCode);
            Assert.Equal("completed", response.Status);
            Assert.Equal("SUCCESS", response.Result);
            // URL is like this with a GUID at the end s3://dem-stg-aps-projects/scriptureappbuilder/eng-5-ebb7a893-b4af-4b14-9bdf-e91d60c0f766
            var projectNamePart = project.AppId + "/" + project.LanguageCode + "-" + response.Id.ToString() + "-";
            Assert.Contains(projectNamePart, response.Url);
        }

        [Theory(Skip = skipIntegrationTest)]
        [InlineData(5)]
        public void DeleteTestProject(int projectId)
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var response = client.DeleteProject(projectId);
            Assert.Equal(System.Net.HttpStatusCode.OK, response);
        }
        [Theory(Skip = skipIntegrationTest)]
        [InlineData(8)]
        public void GetTokenTest(int projectId)
        {
            var client = new BuildEngineApi(BaseUrl, ApiAccessKey);
            var request = new TokenRequest
            {
                Name = "g2=123432423142312345678"
            };
            var response = client.GetProjectAccessToken(projectId, request);
            Assert.NotNull(response.AccessKeyId);
            Assert.NotEmpty(response.AccessKeyId);
            Assert.NotNull(response.SessionToken);
            Assert.NotEmpty(response.SessionToken);
            Assert.NotNull(response.SecretAccessKey);
            Assert.NotEmpty(response.SecretAccessKey);
            Assert.NotNull(response.Expiration);
            Assert.NotEmpty(response.Expiration);
        }
    }
}
