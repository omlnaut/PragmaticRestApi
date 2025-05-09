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

# HATEOAS
## Refactoring before HATEOAS
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

## Implement HATEOAS
### Single Resource
- Begin with GetHabit endpoint
- Represent Single Link
  - new Dto (LinkDto)
    - Href
    - Rel
    - Method
  - Use LinkGenerator for building ressource links
    - GetUriByAction(Context, nameof(Method))
  - LinkService for constructing LinkDtos
    - endpointName, rel, method, object? values, string? controller
    - href by linkGenerator
    - return dto with href and other values
    - register with DI (transient)
  - TryAdd "link" property on expandoObject
    - array of LinkDto
    - example: (nameof(GetHabit), "self", Get, new{id})
    - continue at 7:16
    from 7:16 add more links: update, partial-update, delete
  - Bug: Data shaping is not part of the self-ref link
    -> add fields to values in link generator (why does this work?)
  - Refactor: CreateLinksForHabit(id, fields?)
    - I.e. CreateHabit endpoint. Add List links to HabitDto
  - Continue at 15:30 for collection ressources
  ### Collection Resource
  
  - Link generation challenges
    - Depends on the presence of the id, which might not be present after data shaping
    - ShapeCollectionData needs optional argument for List<LinkDto> generating function
    - When iterating over found entities: add links to each item
    - Add links in PaginationResult->Items
  
  - Structure for collection links
    - Links on the paginationResult itself using same Links property
    - Interface for ILinksResponse for Items Property
    - Method similar to CreateLinksForHabit -> CreateLinksForHabits
      - Self link to GetHabits
      - Arguments: HabitsQueryParameters (page, pageSize, fields, search, sort, type, status)
      - Pay attention to values names (q instead of search)
    
  continue here
  - Additional link types
    - Link: create
    - Page navigation links: 
      - Add parameters to CreateLinksForHabits(hasNext, hasPrevious)
      - If true, add next-page, previous-page link to next/previous page
    - Cross-resource links:
      - Link: upsert-tags from UpsertHabitTags in HabitTagsController
      - Note that id is called habitId here (as defined in the controller endpoint)
      - Needs name of controller - NameOf doesn't work because we need "HabitTags" without "Controller"
      - Add string propertyName to controller

## Content negotiation
- Use custom media type to describe content more precise
- Application should return content based on the "accepted" media type in the request header (=server side content negotiation)
- Configure output formatters:
  ```
  builder.Services.Configure(o => {options.OutputFormatters.OfType().First()
  ```
- Formatter support for custom media types:
  ```
  formatter.SupportedMediaType(application/vnd.dev-habit.hateoas+json)
  ```
  - vnd -> vendor
- Create CustomMediaTypeNames static class for media types
- Add nested "Application" static class
- Only include links if accept header is custom type:
  ```
  [FromHeader(Name=Accept)] string? accept
  ```
- Or add to HabitsQueryParameters (fromHeader works too)
- Use [Produces(mediatypes...)] on endpoint instead of global config

## API versioning
- Example: Renaming fields in HabitWithTagsDto
  - Create v2 dto with updated names (remove utc)
  - Update ProjectToDtoWithTags to v2
  - Create v2 getHabit endpoint
- Problem: runtime sees both endpoints as the same from a route perspective
- Solution: nuget: asp.versioning.mvc solves this via uri
- Define versioning in AddControllers (rename to AddApiServices)
  ```
  builder.Services.AddApiVersioning(o => o.DefaultApiVersion = new ApiVersion(1.0), assumedefault, reportApiVersions, ).AddMvc()
  ```
- Version selector -> CurrentImplementationApiVersionSelector
- ApiVersionReader = new UrlSegmentApiVersionReader()
- In controller:
  ```
  [ApiVersion(1.0)] [ApiVersion(2.0)] Route("v{version:apiVersion}/habits")
  ```
- Use attributes on endpoints:
  ```
  [MapToApiVersion(1.0)] [MapToApiVersion(2.0)]
  ```
- Try request with /v1/
- Using version also alters the hateoas links
