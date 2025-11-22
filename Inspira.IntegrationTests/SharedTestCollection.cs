using Xunit;

namespace Inspira.IntegrationTests
{
    [CollectionDefinition("IntegrationTests")]
    public class SharedTestCollection : ICollectionFixture<IntegrationTestFixture>
    {
    }
}