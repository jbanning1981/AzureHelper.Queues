using AzureClient.Core.Models;
using AzureClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AzureClient.Tests
{
    [Trait("Category", "Unit")]
    public class QueueServiceConstructorTests
    {
        [Fact]
        public void ValidateConfiguration_ThrowsOnNullConfiguration()
        {
            
            var ex = Assert.Throws<ArgumentNullException>( () => new QueueService(null));

            Assert.Equal(nameof(QueueConfiguration), ex.ParamName, ignoreCase: true);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public void ValidateConfiguration_ThrowsOnInvalidConnectionString(string invalidConnectionInfo)
        {
            var expectedType = invalidConnectionInfo == null ? typeof(ArgumentNullException) : typeof(ArgumentException);
            var ex = Assert.Throws(expectedType, () => new QueueService(new QueueConfiguration() { ConnectionString = invalidConnectionInfo }));
            var invalidParamName = (ex as ArgumentException).ParamName;
            
            Assert.IsType(expectedType, ex);
            Assert.Equal(nameof(QueueConfiguration.ConnectionString), invalidParamName);
        }
    }
}
