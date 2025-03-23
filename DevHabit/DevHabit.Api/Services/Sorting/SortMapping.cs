#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable CA1819 // Properties should not return arrays

using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);

public interface ISortMappingDefinition;


public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition

{
    public required SortMapping[] Mappings { get; init; }

}

public sealed class SortMappingProvider(IEnumerable<ISortMappingDefinition> sortMappingDefinitions)
{
    public SortMapping[] GetMappings<TSource, TDestination>()
    {
        var sortMappingDefinition = sortMappingDefinitions
            .OfType<SortMappingDefinition<TSource, TDestination>>()
            .FirstOrDefault();

        if (sortMappingDefinition is null)
        {
            throw new InvalidOperationException($"The mapping from {typeof(TSource).Name} to {typeof(TDestination).Name} is not registered ");
        }

        return sortMappingDefinition.Mappings;
    }
}

internal static class QueryableExtensions
{
    public static IQueryable<T> ApplySort<T>(this IQueryable<T> query,
                                             string? sort,
                                             SortMapping[] mappings,
                                             string defaultOrderBy = "Id")
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return query.OrderBy(defaultOrderBy);
        }

        var sortFields = sort.Split(',')
                             .Select(s => s.Trim())
                             .Where(s => !string.IsNullOrWhiteSpace(s))
                             .ToArray();

        var orderByParts = new List<string>();

        foreach (var sortField in sortFields)
        {
            var (name, isDescending) = ParseSortField(sortField);

            var mapping = mappings.First(m => m.SortField == name);

            var direction = (mapping.Reverse, isDescending) switch
            {
                (true, true) => "asc",
                (true, false) => "desc",
                (false, true) => "desc",
                (false, false) => "asc",
            };

            var fieldString = $"{name} {direction}";
            orderByParts.Add(fieldString);
        }

        var orderBy = string.Join(",", orderByParts);

        return query.OrderBy(orderBy);
    }

    /// <summary>
    /// field is of type "name" or "name asc" or "name desc". If not given, descending by default
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private static (string SortField, bool IsDescending) ParseSortField(string field)
    {
        var parts = field.Split(" ");
        var name = parts[0];

        if (parts.Length == 1)
        {
            return (name, true);
        }

        var sortType = parts[1];

        var isDescending = sortType.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return (name, isDescending);

    }
}