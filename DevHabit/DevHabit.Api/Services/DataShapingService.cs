using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace DevHabit.Api.Services;
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static
#pragma warning disable CA1822 // Mark members as static

public class DataShapingService
{

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyInfoCache = new();

    public List<ExpandoObject> ShapeData<T>(List<T> entities, string? fields)
    {

        var (_, propertyInfos) = ExtractFieldsAndPropertyInfo<T>(fields);


        var shapedObjects = new List<ExpandoObject>();

        foreach (var entity in entities)
        {
            IDictionary<string, object?> shaped = new ExpandoObject();

            foreach (var propertyInfo in propertyInfos)
            {
                shaped[propertyInfo.Name] = propertyInfo.GetValue(entity);
            }

            shapedObjects.Add((ExpandoObject)shaped);
        }

        return shapedObjects;
    }


    private (HashSet<string>, PropertyInfo[]) ExtractFieldsAndPropertyInfo<T>(string? fields)

    {
        var propertyInfos = propertyInfoCache.GetOrAdd(typeof(T), t =>
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToArray();
        });

        var fieldSet = fields?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                             .Select(f => f.Trim())
                             .ToHashSet(StringComparer.OrdinalIgnoreCase)
                             ?? [];

        if (fieldSet.Count != 0)
        {
            propertyInfos = propertyInfos.Where(p => fieldSet.Contains(p.Name)).ToArray();
        }

        return (fieldSet, propertyInfos);
    }

    public bool Validate<T>(string? fields)
    {
        var (fieldSet, propertyInfo) = ExtractFieldsAndPropertyInfo<T>(fields);

        if (fieldSet.Count == 1)
        {
            return true;
        }

        return fieldSet.All(f => propertyInfo.Select(p => p.Name).Contains(f, StringComparer.OrdinalIgnoreCase));
    }
}