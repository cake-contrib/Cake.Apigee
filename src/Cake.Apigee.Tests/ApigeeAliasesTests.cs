using System;
using System.Linq;

using Xunit;

namespace Cake.Apigee.Tests
{
    public class ApigeeAliasesTests
    {        
        [Fact]
        public void WithoutSettings_ShouldImportProxy()
        {
            // Arrange
            var fixture = new ApigeeAliasesFixture();            
            ApigeeAliases.ApigeeProxyManagementService = fixture.ApigeeProxyManagementService;

            fixture.UseSuccessfulImportResponse();    

            // Act
            ApigeeAliases.ImportProxy(fixture.ContextMock.Object, "org", "proxy", fixture.GetProxyZipFilePath());

            // Assert
            Assert.Equal(new Uri("https://api.enterprise.apigee.com//v1/organizations/org/apis?action=import&name=proxy"), fixture.RequestUrl);
        }

        [Fact]
        public void WithCredentials_ShouldImportProxyWithCredentials()
        {
            // Arrange     
            var fixture = new ApigeeAliasesFixture();
            ApigeeAliases.ApigeeProxyManagementService = fixture.ApigeeProxyManagementService;

            fixture.UseSuccessfulImportResponse();

            // Act
            var credentials = new Credentials { Username = "testUser", Password = "testPassword" };
            ApigeeAliases.ImportProxy(fixture.ContextMock.Object, "org", "proxy", fixture.GetProxyZipFilePath(), new ImportProxySettings { Credentials = credentials });

            // Assert
            Assert.Equal("Basic", fixture.FakeResponseHandler.Requests.First().Headers.Authorization.Scheme);
        }
    }
}
