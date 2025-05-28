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
- Continue here
- Replace UriSegmentReader with Combined approach:
  ```
  ApiVersionReader.Combine(new MediaTypeApiVersionReader + new MediaReaderBuilder.Template)
  ```
- Define template format:
  ```
  "application/vnd.deb-habit.hateoas.{version}+json"
  ```
- Put custom media type back to global config (formatter.SupportedMediaTypes from OutputFormatters)
- Add new CustomTypes:
  - Hateoas1, Hateoas2 with version as in template 
  - JsonV1, JsonV2 /json;v=X
- Add support for all of these media types
- Get rid of hardcoded version in [Route], leave [ApiVersion(1.0)]
- Specify only on endpoint
- Try requests with versioned media type
- Caution: breaks other endpoints, because UseHighestVersion is 2.0. Could use DefaultSelector instead

# Auth Stuff
## User Ressource

### Entity Setup
- User Entity Id, Email, Name, CreatedAtUtc, UpdatedAtUtc, IdentityId (from the identityProvider (like azure, keycloak, ...)
- start with asp.net identity as provider
- UserDto (id, mail, name, created+updatedAtUtc)
- UserQueries (projectToDto)
- Split all entities in their own files
- db config max length on id, mail, IdentityId, name index+unique on email + identityId
### Controller changes
- UserController GetUserById
- Custom Media Types (habitsController i.e.) Produces: Json, JsonV1, JsonV2, HateoasJson+v1+v2
- IncludeLinks on HabitsQueryParameters (what's the difference in media types then?)
- QueryParameters implements AcceptHeaderDto IncludeLinks is property, check if SubTypeWithoutSuffix is HateoasSubType
- Also in GetHabitById, introduce strongly typed argument
- TagsController Prodcues v1 media types

## IdentityCore introduction
- Custom dbContext for Identity (with own schema=identity)
- ApplicationIdentityDbContext : IdentityDbContext (nuget IdentityEntityFrameworkCore)
- constructor injects DbContextOptions<AppIdenDbContext>, pass to base
- OnModelCreating HasDefaultSchema(Identity)
- DI copy setup but set other schema
- new DI extension AddAuthenticationServices on builder AddIdentity<IdentityUser, IdentityRole>() .AddEntityFrameworkStores<customdb>
- dotnet ef migrations add AddIdentity -context ApplicationIdentityDbContext -o Migrations/Identity
- ApplyMigrationAsync, add IdentityDbContext as copy of existing migration logic

## User Registration

- **AuthController** 
  - Route "auth"
  - Use `[AllowAnonymous]` attribute

- **Dependencies**
  - Inject UserManager, DbContext, IdentityDbContext
  - UserManager allows interaction with the users table

- **Endpoint**
  - POST endpoint "register" - `Task<IActionResult> Register(RegisterUserDto)`

- **RegisterUserDto**
  - Properties: Email, Name, Password, ConfirmPassword

- **Implementation Flow**
  - Create identity user
  - Create app user
  - Return Ok()

- **Process Details**
  - Create new IdentityUser -> Call CreateAsync
  - Check result for succeeded
  - If not successful: Return Problem(detail: "unable to register", statusCode: 400, extensions: errors.ToDictionary[Code->description])
  - Use UserMappings -> ToEntity(this RegisterUserDto)
  - Generate ID: `Id = $"u_{Guid.CreateVersion7}"`
  - Set User.IdentityId from identityUser
  - Return user.Id for testing

- **Transaction Handling**
  - Problem: Two dbContexts, operations might run in separate transactions
  - Solution:
    ```csharp
    using transaction = await Database.BeginTransactionAsync()
    await transaction.CommitAsync()
    ```
  - To sync with appDbContext:
    ```csharp
    DbContext.Database.SetDbConnection(identity.GetConnection())
    .UseTransactionAsync(transaction.GetDbTransaction)
    ```

- **Testing**
  - Try with password "123a"

## AccessToken

- Install nuget JwtBearer (AspNetCore)

- DI: Configure Authentication and JWT Bearer
  ```csharp
  AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
  AddJwtBearer(options => options.TokenValidParameters = new() { ...})
  ```

- Create JwtAuthOptions class in settings folder
  - Properties: Issuer, Audience, Key, ExpirationInMinutes(int)

- Add JWT section in appsettings.development.json
  - Fields: Issuer, Audience, Key, ExpirationInMinutes, RefreshTokenExpirationDays
  - Example values:
    - Issuer: dev-habit.api
    - Audience: dev-habit.app

- Register Options in DI
  - Use configure->config.GetSection for global DI
  - For local setup, use getSection().Get

- Configure TokenValidationParameters
  - ValidIssuer, ValidAudience
  - IssuerSigningKey (new SymmetricSecurityKey(UTF8.GetBytes(key)))

- Call AddAuthorization()

- Add middleware in correct order
  - UseAuthentication
  - UseAuthorization
  - Order is important!

- Create TokenProvider service
  - Inject IOptions<JwtAuthOptions>

- Create token-related DTOs
  - TokenRequest (userId, Email)
  - AccessTokensDto (AccessToken, RefreshToken)

- In TokenProvider service
  - Create method: AccessToken Create(request)
  - Initially return empty strings

- Add private helper methods
  - GenerateAccessToken(request)
  - GenerateRefreshToken()

- Implement access token generation
  - Create SymmetricSecurityKey from key bytes
  - Create SigningCredentials with HmacSha256
  - Generate claims list:
    - JwtRegisteredClaimNames.Sub (UserId)
    - Email claim
  - Create token descriptor:
    - ClaimsIdentity with claims
    - Set expiration (now + expirationInMinutes)
    - Add SigningCredentials
    - Set Issuer and Audience
  - Create token using JsonWebTokenHandler

- Move DTOs to their own files in Dtos-Auth folder

- Register TokenProvider as transient service

- Update AuthController register endpoint
  - Create access tokens at end of registration
  - Use: tokenProvider.Create(new TokenRequest(identityUser.Id, email))
  - Return accessTokenDto

- Create login endpoint
  - Return type: ActionResult<AccessTokensDto>
  - Create LoginUserDto class (email, password)
  - Implementation:
    - Find identityUser by email using UserManager
    - Check for null and validate password
    - If invalid, return Unauthorized()
    - Create token request and return access tokens

- Test by adding [Authorize] attribute to controllers

- Fix 404 issue on unauthorized requests
  - By default, unauthorized requests redirect to login page
  - Configure AddAuthentication:
    ```csharp
    o => o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme
         o => o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme
    ```

## RefreshToken

- **RefreshToken Entity**
  - Properties: Guid Id, UserId, Token, ExpiresAtUtc
  - Navigation property: IdentityUser User

- **Configure Database (ApplicationIdentityDbContext)**
  - DbSet<RefreshToken> RefreshTokens
  - Entity configuration:
    - Id as key
    - UserId and Token with maxlength (300, 100)
    - Token unique index
    - 1-to-many relationship: RefreshToken has many User with foreign key and cascade delete

- **Migration**
  - Create migration with: `-Context ApplicationIdentityDbContext -o Migrations/Identity`

- **Token Generation**
  - Generate RefreshToken in TokenProvider using `RandomNumberGenerator.GetBytes(32)` and `Convert.ToBase64String`

- **Controller Integration**
  - AuthController Register: Move token generation before commit transaction
  - Create RefreshTokenEntity with ExpiresAt from JwtAuthOptions
  - Add to identityDbContext and save
  - Apply same pattern for login endpoint

  - **Refresh Endpoint**
  - Create new endpoint: `Task<ActionResult<AccessTokenDto>> Refresh(RefreshTokenDto)`
  - Get refresh token from dbContext
  - If not found -> return Unauthorized
  - Check expiration -> if expired, return Unauthorized
  - Otherwise:
    - Create new tokens from tokenProvider(request)
    - Update info in refreshToken (Token and expires date)
    - Save changes
    - Return Ok with new tokens

## Resource Protection

- "me" endpoint in UserController 
  - Use dbContext.Users to match Id
  - Get Id from HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)

- ExtensionMethod to get IdentityId 
  - Create ClaimsPrincipalExtensions (everything starting from FindFirstValue)

- Create UserContext service
  - Dependencies: httpContextAccessor, AppDbContext, IMemoryCache
  - Implementation:
    - GetUserIdAsync - get identityId from httpContext
    - Fetch user from database if not in cache (getOrCreateAsync)
  - Use in UserController

- Authorization enforcement
  - In get/id endpoint compare GetUserIdAsync with passed id, return forbidden if not matching

- DI setup
  - AddMemoryCache
  - AddScoped for UserContext

CONTINUE HERE
- Protect other resources
  - Habit Entity -> Add UserId
  - Tag Entity -> Add UserId

- Update database configurations
  - MaxLen on UserId (500)
  - 1 Habit has one User, User has many habits, add ForeignKey
  - Tag name uniqueness as combination of userId+tagName

- Create Migration
  - Update migrationBuilder.Sql to delete all habits, tags, habittags

- HabitsController updates
  - Add UserContext dependency
  - Fetch UserId, use in where statements
  - Add to all fetching/updating endpoints
  - In create endpoint: Add UserId in ToEntity

- Update TagsController with similar changes