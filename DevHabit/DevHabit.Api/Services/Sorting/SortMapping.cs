#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable CA1819 // Properties should not return arrays

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