#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable CA1819 // Properties should not return arrays

using System.Linq.Dynamic.Core;

namespace DevHabit.Api.Services.Sorting;

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

    public bool ValidateMappings<TSource, TDestination>(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
        {
            return true;
        }

        var mappings = GetMappings<TSource, TDestination>();
        var sortFields = sort.Split(",")
                             .Select(s => s.Trim()
                                           .Split(" ")[0])
                            .Where(s => !string.IsNullOrWhiteSpace(s))
                            .ToList();

        return sortFields.All(sf => mappings.Any(m => m.SortField.Equals(sf, StringComparison.OrdinalIgnoreCase)));
    }
}
