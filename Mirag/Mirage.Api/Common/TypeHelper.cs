namespace Mirage.Api.Common
{
    public static class TypeHelper
    {
        public static string GetNameCompletely(this Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeName = type.GetGenericTypeDefinition().Name;
                var index = genericTypeName.IndexOf('`');
                if (index > 0)
                {
                    genericTypeName = genericTypeName.Substring(0, index);
                }
                string genericArgs = string.Join(", ", type.GetGenericArguments().Select(t => t.GetNameCompletely()));
                return $"{genericTypeName}<{genericArgs}>";
            }
            return type.Name;
        }
    }
}