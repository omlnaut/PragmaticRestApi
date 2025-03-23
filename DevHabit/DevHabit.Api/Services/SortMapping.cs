namespace DevHabit.Api.Services.Sorting;

public sealed record SortMapping(string SortField, string PropertyName, bool Reverse = false);

#pragma warning disable CA1040 // Avoid empty interfaces
public interface ISortMappingDefinition;
#pragma warning restore CA1040 // Avoid empty interfaces


#pragma warning disable S2326 // Unused type parameters should be removed
public sealed class SortMappingDefinition<TSource, TDestination> : ISortMappingDefinition
#pragma warning restore S2326 // Unused type parameters should be removed

{
#pragma warning disable CA1819 // Properties should not return arrays
    public required SortMapping[] Mappings { get; init; }
#pragma warning restore CA1819 // Properties should not return arrays

}