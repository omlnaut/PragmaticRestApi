#pragma warning disable CA1040 // Avoid empty interfaces
#pragma warning disable S2326 // Unused type parameters should be removed
#pragma warning disable CA1819 // Properties should not return arrays

namespace DevHabit.Api.Services.Sorting;

public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
{
    public required SortMapping[] Mappings { get; init; }

}
