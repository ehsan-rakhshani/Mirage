using Mirage.Api.Infrastructure.Services.MockServer;

namespace Mirage.Api.Infrastructure.Attributes
{
    public class SampleAttribute<T> : Attribute where T : MyAbstractClass, new()
    {
       
    }
}