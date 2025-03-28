using System.Collections.Concurrent;
using System.Dynamic;
using System.Reflection;

namespace DevHabit.Api.Services;
#pragma warning disable S2325 // Methods and properties that don't access instance data should be static

public class DataShapingService
{

    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> propertyInfoCache = new();

    public List<ExpandoObject> ShapeData<T>(List<T> entities, string? fields)
    {

        var propertyInfos = propertyInfoCache.GetOrAdd(typeof(T), t =>
        {
            return t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .ToArray();
        });

        if (fields is not null)
        {
            var fieldSet = fields.Split(',').Select(f => f.Trim()).ToHashSet(StringComparer.InvariantCultureIgnoreCase);
            propertyInfos = propertyInfos.Where(p => fieldSet.Contains(p.Name)).ToArray();
        }


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

}