using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mirage.Api.Infrastructure.Services.ObjectGenerator;
public static class FakerService
{
    // حداکثر عمق بازگشتی برای جلوگیری از ایجاد نمونه‌های تو در تو بیش از حد
    private const int MaxRecursionDepth = 3;

    public static object CreateMockInstance(Type type, int currentDepth = 0)
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

        // مدیریت کلاس‌ها و رکوردها (Custom Classes/Records)
        object instance = null;
        // ابتدا سعی می‌کنیم از سازنده بدون پارامتر استفاده کنیم
        ConstructorInfo ctor = type.GetConstructor(Type.EmptyTypes);
        if (ctor != null)
        {
            instance = Activator.CreateInstance(type);
        }
        else
        {
            // در صورتی که سازنده بدون پارامتر موجود نباشد، از سازنده‌ای با کمترین تعداد پارامتر استفاده می‌کنیم
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
            return null;

        // مقداردهی به خواص عمومی (Properties) که قابلیت set دارند
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
                // در صورت خطا مقداردهی نادیده گرفته می‌شود
            }
        }

        // مقداردهی به فیلدهای عمومی (Fields)
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
                // در صورت خطا مقداردهی نادیده گرفته می‌شود
            }
        }

        return instance;
    }
}
