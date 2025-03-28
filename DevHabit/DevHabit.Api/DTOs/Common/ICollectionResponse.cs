namespace DevHabit.Api.DTOs.Common;

public interface ICollectionResponse<T>
{
    public List<T> Items { get; init; }
}
