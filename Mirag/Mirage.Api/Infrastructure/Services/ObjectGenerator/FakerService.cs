using AutoFixture.Kernel;
using AutoFixture;
using Newtonsoft.Json;

namespace Mirage.Api.Infrastructure.Services.ObjectGenerator
{
    public class FakerService
    {
        public static string CreateInstance(string typeName)
        {
            var type = Type.GetType(typeName);

            if (type == null)
            {
                throw new ArgumentException($"نوع '{typeName}' پیدا نشد.");
            }

            var fixture = new Fixture();
            var obj = fixture.Create(type, new SpecimenContext(fixture));

            return JsonConvert.SerializeObject(obj);
        }
    }
}