# Pagination

- Cursor based pagination as first example
  
## Implementation task
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

# Data Shaping
- Select fields like in a graphql query
- extend QueryParameters by Fields: str
- remove .CreateAsync Call in pagination
  - totalCount and habit page manually in controller
- new type in services folder, DataShapingService
  - List<ExpandoObject> ShapeData
  - ShapeData(entities, string)
  - fields to hashset (split on ,)
  - get propertyInfos via GetPropeties (public and instance)
  - cache via static ConcurrentDictionary PropertiesCache -> GetOrAdd
- for each entity
  - new expando object (dict<string, object?>)
  - re-add to shapedObje
  - filter propertyInfos by name
  - add value via GetValue
- register as transient sevice
- call in PaginationResult -> Items
- make fields argument nullable in DataShapingService
- return type of controller is now just ActionResult

- unknown fields should produce 400 Bad Request
- add Validate method to shaping service
  - if empty: true
  - assert that all entries of fieldsSet are in propertyInfos (compare by name, ordinalCaseIgnore)
  - validate in controller -> return 400 Bad Requesto

  Serialization
  - In NewtonSoft using, options.SerializerSettings.ContractResolver -> CamelCasePropertyNamesj

  Now for a single Habit
  - add fields as arguent for endpoint
  - validate fields
  - ShapeData only takes collection -> add support for shaping a single item

  # Refactoring before HATEOAS
  - program.cs is getting hard to read
  - remove all weatherforecast stuff
  - dependencyInjection class as collector for registration stuff
    - AddControllers
      - normal controller registration
      - openapi/swagger
    - ProblemDetails/ExceptionHandler
    - AddDatabase
    - AddObervability
    - AddApplicatinServices
      - VAlidator
      - Services
      - ...