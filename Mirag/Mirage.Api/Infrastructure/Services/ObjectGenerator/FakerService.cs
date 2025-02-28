using AutoFixture;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mirage.Api.Infrastructure.Services.ObjectGenerator;

public class FakerService
{
    private readonly Fixture _fixture;
    private readonly Random _random;

    public FakerService()
    {
        _fixture = new Fixture();
        _random = new Random();
    }

    public string CreateFakeData(Type type)
    {
        object result;

        if (IsEnumerableType(type, out Type? itemType) && itemType != null)
        {
            // Dynamically call the generic method for the correct item type
            var method = typeof(FakerService).GetMethod(nameof(CreateFakeCollection), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            var genericMethod = method?.MakeGenericMethod(itemType);
            result = genericMethod?.Invoke(this, null) ?? new List<object>();
        }
        else
        {
            result = CreateSingleFakeObject(type);
        }

        return JsonConvert.SerializeObject(result, Formatting.Indented);
    }

    private bool IsEnumerableType(Type type, out Type? itemType)
    {
        if (type.IsGenericType && typeof(IEnumerable<>).IsAssignableFrom(type.GetGenericTypeDefinition()))
        {
            itemType = type.GetGenericArguments()[0];
            return true;
        }

        itemType = null;
        return false;
    }

    private object CreateSingleFakeObject(Type type)
    {
        var methodInfo = typeof(Fixture).GetMethods()
            .FirstOrDefault(m => m.Name == "Create" &&
                                 m.IsGenericMethod &&
                                 m.GetParameters().Length == 0);

        if (methodInfo == null)
            throw new InvalidOperationException("Method 'Create' not found.");

        var genericMethod = methodInfo.MakeGenericMethod(type);
        return genericMethod.Invoke(_fixture, null) ?? $"No fake data could be generated for {type.Name}";
    }

    private List<T> CreateFakeCollection<T>()
    {
        int count = _random.Next(1, 20); 
        var randomList = new List<T>();

        for (int i = 0; i < count; i++)
        {
            randomList.Add(_fixture.Create<T>());
        }

        return randomList;
    }
}
