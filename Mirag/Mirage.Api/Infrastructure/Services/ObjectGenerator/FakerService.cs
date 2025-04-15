using Mirage.Api.Infrastructure.Services.MockServer;
using System.Collections;
using System.Reflection;

namespace Mirage.Api.Infrastructure.Services.ObjectGenerator;
public static class FakerService
{
    private const int MaxRecursionDepth = 30;

    public static object? CreateMockInstance(Type type, int currentDepth = 0)
    {

        if (currentDepth > MaxRecursionDepth)
            return null;

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        if (type == typeof(string))
        {
            return "mockString";
        }

        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
        {
            return CreateMockInstance(underlyingType, currentDepth + 1);
        }

        if (type.IsArray)
        {
            Type elementType = type.GetElementType();
            var arrayInstance = Array.CreateInstance(elementType, 1);
            arrayInstance.SetValue(CreateMockInstance(elementType, currentDepth + 1), 0);
            return arrayInstance;
        }

        if (type.IsGenericType)
        {
            Type genericDef = type.GetGenericTypeDefinition();

            if (genericDef == typeof(List<>))
            {
                var listInstance = (IList)Activator.CreateInstance(type);
                Type itemType = type.GetGenericArguments()[0];
                listInstance.Add(CreateMockInstance(itemType, currentDepth + 1));
                return listInstance;
            }

            if (genericDef == typeof(Dictionary<,>))
            {
                var dictInstance = (IDictionary)Activator.CreateInstance(type);
                Type keyType = type.GetGenericArguments()[0];
                Type valueType = type.GetGenericArguments()[1];
                var keyInstance = CreateMockInstance(keyType, currentDepth + 1);
                var valueInstance = CreateMockInstance(valueType, currentDepth + 1);
                if (keyInstance != null)
                    dictInstance.Add(keyInstance, valueInstance);
                return dictInstance;
            }
        }

        object instance = null;
        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
        {
            instance = Activator.CreateInstance(type);
        }
        else
        {
            var ctors = type.GetConstructors().OrderBy(c => c.GetParameters().Length).ToArray();
            if (ctors.Any())
            {
                ctor = ctors.First();
                var parameters = ctor.GetParameters();
                var args = parameters.Select(p => CreateMockInstance(p.ParameterType, currentDepth + 1)).ToArray();
                instance = ctor.Invoke(args);
            }
        }

        if (instance == null)
            return string.Empty;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanWrite);
        foreach (var prop in properties)
        {
            try
            {
                object propValue = CreateMockInstance(prop.PropertyType, currentDepth + 1);
                prop.SetValue(instance, propValue);
            }
            catch
            {
                continue;
            }
        }

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            try
            {
                object fieldValue = CreateMockInstance(field.FieldType, currentDepth + 1);
                field.SetValue(instance, fieldValue);
            }
            catch
            {
                continue;
            }
        }

        return instance;
    }
}