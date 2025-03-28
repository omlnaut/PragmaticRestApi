## Pagination

- Cursor based pagination as first example
  
### Implementation task
- extend GetHabits
- Update QueryParameters by page and pagesize
- Count records after filtering for totalCount
- then Sort -> Pagination (skip/take) -> Projection
- IColledctionResponse<T> as generic pagination wrapper (list -> Items)
  - common (under dto)
  - HabitsCollectionDto, TagsCollectionDto
- PaginationResult<T> -> Common
  - Page, PageSize, TotalCount, TotalPages, HasPreviousPage, HasNextPage
  - static create Async (queryable, page, pagesize as arguments)
- Apply all other operations, then apply pagination via the new method