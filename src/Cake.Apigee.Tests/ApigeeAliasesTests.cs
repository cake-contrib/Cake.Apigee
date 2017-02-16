using System;
using System.Linq;

using Cake.Apigee.Services;

using Xunit;

namespace Cake.Apigee.Tests
{
    public class ApigeeAliasesTests : IClassFixture<ApigeeAliasesFixture>
    {
        private readonly ApigeeAliasesFixture fixture;

        public ApigeeAliasesTests(ApigeeAliasesFixture fixture)
        {
            this.fixture = fixture;
            ApigeeAliases.ApigeeProxyManagementService = this.fixture.ApigeeProxyManagementService;
        }

        [Fact]
        public void WithoutSettings_ShouldImportProxy()
        {   
            // Arrange     
            this.fixture.UseSuccessfulImportResponse();    

            // Act
            ApigeeAliases.ImportProxy(fixture.ContextMock.Object, "org", "proxy", fixture.GetProxyZipFilePath());

            // Assert
            Assert.Equal(new Uri("https://api.enterprise.apigee.com//v1/organizations/org/apis?action=import&name=proxy"), fixture.RequestUrl);
        }

        [Fact]
        public void WithCredentials_ShouldImportProxyWithCredentials()
        {
            // Arrange     
            this.fixture.UseSuccessfulImportResponse();

            // Act
            var credentials = new Credentials { Username = "testUser", Password = "testPassword" };
            ApigeeAliases.ImportProxy(fixture.ContextMock.Object, "org", "proxy", fixture.GetProxyZipFilePath(), new ImportProxySettings { Credentials = credentials });

            // Assert
            Assert.Equal("Basic", fixture.FakeResponseHandler.Requests.First().Headers.Authorization.Scheme);
        }
    }
}
