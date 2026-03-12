# TetraControl2Connect - Copilot Instructions

This document provides comprehensive guidance for AI-assisted development on the TetraControl2Connect project.

## Project Overview

TetraControl2Connect is a .NET 10.0 connector application that bridges TetraControl (a TETRA radio system) with Feuer Software's Connect platform. It handles real-time vehicle tracking, user status management, alarms, and incident operations through WebSocket communication and REST APIs.

**Key Projects:**
- `TetraControl2Connect` - Main web application (ASP.NET Core + hosted services)
- `TetraControl2Connect/AdminUI` - Nuxt 3 admin frontend (SSG)
- `TetraControl2Connect.Shared` - Shared options, models, and constants
- `TetraControl2Connect.Test` - xUnit test suite

---

## 1. Coding Conventions & C# Style

### Naming Conventions

**Namespaces:**
- Follow company convention: `FeuerSoftware.TetraControl2Connect[.Subsystem]`
- Examples:
  - `FeuerSoftware.TetraControl2Connect.Services`
  - `FeuerSoftware.TetraControl2Connect.Models.Connect`
  - `FeuerSoftware.TetraControl2Connect.Models.TetraControl`

**Classes & Methods:**
- **PascalCase** for class names, method names, and public properties
- **camelCase** for private fields and method parameters
- **Service interfaces** start with `I`: `IVehicleService`, `IConnectApiService`, `ITetraControlClient`

**Private Fields:**
- Prefix with underscore: `_log`, `_connectApiService`, `_vehicles`
- Exception: fields initialized in constructor parameters with null-throw pattern don't need prefix

**Constants:**
- Use `static` keyword: `public const double PositionTolerance = 0.00009d`
- Use `static` properties with backing fields: `public static string Version => ...`

### Formatting & Whitespace

- **Nullable reference types enabled** (`<Nullable>enable</Nullable>`)
- **Implicit usings enabled** (`<ImplicitUsings>enable</ImplicitUsings>`)
- Use **empty lines** between logical sections in methods
- **No extra blank lines** between related properties/fields
- Multi-line property initializers: closing brace on new line

### Modern C# Patterns Used

**Primary Constructor Pattern:**
```csharp
public class VehicleService(
    ILogger<VehicleService> log,
    IConnectApiService connectApiService,
    IOptions<ConnectOptions> connectOptions,
    IOptions<ProgramOptions> programOptions) : IVehicleService
{
    private readonly ILogger<VehicleService> _log = log ?? throw new ArgumentNullException(nameof(log));
    // ... other fields
}
```

**Records for Models:**
- Use `public record` for data transfer objects and models
- Example: `public sealed record VehicleModel` and `public record OperationModel`
- Include optional nullable properties with `?` and initialize with empty defaults: `string = string.Empty`
- Can be sealed: `public sealed record VehicleModel`

**Target-typed new expressions:**
- Use `new()` without type repetition: `new() { /* init */ }`
- Example: `HashSet<VehicleModel> _vehicles = [];` (empty collection)

**Collection initializers:**
```csharp
public List<OperationPropertyModel> Properties { get; set; } = [];
Sites = [new Site() { Key = testkey1, Name = "TestStandort1" }]
```

**Null-coalescing & null-checking:**
```csharp
private readonly ILogger<VehicleService> _log = log ?? throw new ArgumentNullException(nameof(log));
if (!_isInitialized) throw new InvalidOperationException("Service not initialized.");
ArgumentNullException.ThrowIfNull(source);
```

**String interpolation:**
- Always use `$"..."` for logging and messages
- Example: `_log.LogInformation($"Sent position update for vehicle '{vehicle.Description}' ISSI '{vehicle.RadioId}' to Connect.");`

**Async patterns:**
- Use `.ConfigureAwait(false)` when not requiring synchronization context
- Return `Task` and `Task<T>` with async methods marked `async`

---

## 2. Architecture Patterns

### Service-Oriented Architecture

The application uses a **layered service architecture**:

1. **Client Layer:** `TetraControlClient` (WebSocket connection to TETRA system)
2. **Service Layer:** Multiple domain services processing data
3. **Integration Layer:** `ConnectApiService` (REST API to Connect platform)
4. **Hosted Service:** `Agent` (coordinator running in background)

### Dependency Injection Pattern

**All services use constructor injection:**
```csharp
public class VehicleService(
    ILogger<VehicleService> log,
    IConnectApiService connectApiService,
    IOptions<ConnectOptions> connectOptions,
    IOptions<ProgramOptions> programOptions) : IVehicleService
```

**DI Registration in Program.cs:**
- Services registered as **singletons**: `.AddSingleton<IVehicleService, VehicleService>()`
- All services must have corresponding interfaces
- Configuration options registered as: `.Configure<ProgramOptions>(hostContext.Configuration.GetRequiredSection(ProgramOptions.SectionName))`

**HttpClient Factory Pattern:**
```csharp
using var httpClient = _httpClientFactory.CreateClient(nameof(IConnectApiService));
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
```

### Interface-Implementation Pattern

**Every service has an interface:**
- Interfaces start with `I`: `IVehicleService`, `IConnectApiService`
- Interfaces define public contract only
- Implementation classes are concrete: `VehicleService`, `ConnectApiService`
- Interfaces often inherit `IDisposable` if managing resources

**Example Interface:**
```csharp
public interface IVehicleService : IDisposable
{
    Task HandleVehiclePosition(TetraControlDto dto);
    Task HandleVehicleStatus(TetraControlDto dto);
    Task Initialize();
}
```

### Internal State Management

Services maintain **thread-safe internal state:**
- `HashSet<T>` for unique collections: `private readonly HashSet<VehicleModel> _vehicles = [];`
- `ConcurrentDictionary<K, V>` for thread-safe lookups: `private readonly ConcurrentDictionary<string, int> _vehicleStatusCache = new();`
- **Initialize pattern:** Services must call `Initialize()` before use
- **Disposal pattern:** Implement `Dispose()` to clean up subscriptions/resources

---

## 3. Namespace Conventions

Follow strict namespace hierarchy:

```
FeuerSoftware.TetraControl2Connect/
├── Services/                          → FeuerSoftware.TetraControl2Connect.Services
├── Models/
│   ├── Connect/                       → FeuerSoftware.TetraControl2Connect.Models.Connect
│   └── TetraControl/                  → FeuerSoftware.TetraControl2Connect.Models.TetraControl
├── Extensions/                        → FeuerSoftware.TetraControl2Connect.Extensions
├── Converters/                        → FeuerSoftware.TetraControl2Connect.Converters
├── Constants.cs                       → FeuerSoftware.TetraControl2Connect
└── Program.cs                         → FeuerSoftware.TetraControl2Connect

TetraControl2Connect.Shared/
└── Options/                           → FeuerSoftware.TetraControl2Connect.Shared.Options
    └── Models/                        → FeuerSoftware.TetraControl2Connect.Shared.Options.Models

TetraControl2Connect.Test/
├── Services/                          → FeuerSoftware.TetraControl2Connect.Test.Services
├── Extensions/                        → FeuerSoftware.TetraControl2Connect.Test.Extensions
└── Helper/                            → FeuerSoftware.TetraControl2Connect.Test.Helper
```

**Root Namespace:** Automatically set via MSBuild:
- `<RootNamespace>FeuerSoftware.$(MSBuildProjectName.Replace(" ", "_"))</RootNamespace>`

---

## 4. Test Conventions

### Test Framework & Tools

- **Framework:** xUnit (`[Fact]`, `[Theory]`, `[InlineData(...)]`)
- **Mocking:** Moq - `new Mock<IInterface>()`
- **Assertions:** FluentAssertions - `.Should().Be()`, `.Should().HaveCount()`, `.Should().NotBeNull()`
- **Test Data Generation:** Bogus - `new Faker("de")` for German locale
- **Test Helpers:** Custom `TestHelper` static class with factory methods

### Test Naming & Structure

**Test Class Naming:**
- `ServiceName` + `Test`: `VehicleServiceTest`, `UserServiceTest`
- Tests inherit no base class
- Located in mirror directory: `TetraControl2Connect.Test/Services/`

**Test Method Naming:**
- `[MethodName]_[Scenario]_[ExpectedResult]`
- Example: `HandlePosition_WithCache_WithinTolerance`
- Single responsibility per test

**Test Structure Pattern:**
```csharp
[Fact]
public async Task GetAccessTokensForVehicle_Positive()
{
    // 1. Setup (create mocks and test data)
    var log = new Mock<ILogger<VehicleService>>();
    var testkey1 = Faker.Random.AlphaNumeric(500);
    var connectOptions = new ConnectOptions() { Sites = [...] };
    var vehicles = TestHelper.GenerateVehicles(100);
    
    // 2. Mock Setup
    var connectOptionsMock = new Mock<IOptions<ConnectOptions>>();
    connectOptionsMock.Setup(o => o.Value).Returns(connectOptions);
    
    var connectApiService = new Mock<IConnectApiService>();
    connectApiService
        .Setup(c => c.GetVehicles(It.Is<string>(k => ...)))
        .ReturnsAsync(vehicles);
    
    // 3. Execute
    var service = new VehicleService(
        log.Object,
        connectApiService.Object,
        connectOptionsMock.Object,
        programOptionsMock.Object);
    
    await service.Initialize();
    var tokens = service.GetAccessTokensForVehicle(testVehicle.RadioId);
    
    // 4. Assert
    tokens.Should().HaveCount(2);
    tokens.Should().Contain(testkey1);
}
```

### Mocking Patterns

**Interface Options:**
```csharp
var optionsMock = new Mock<IOptions<ConnectOptions>>();
optionsMock.Setup(o => o.Value).Returns(connectOptions);
// Use: optionsMock.Object
```

**It.Is<T> for predicates:**
```csharp
connectApiService
    .Setup(c => c.GetVehicles(It.Is<string>(k => connectOptions.Sites.Select(s => s.Key).Contains(k))))
    .ReturnsAsync(vehicles);
```

**Callback for method side effects:**
```csharp
var numberOfPositionUpdateCalls = 0;
connectApiService
    .Setup(c => c.PostVehiclePosition(testkey1, issi, It.IsAny<StatusPositionModel>()))
    .Callback(() => numberOfPositionUpdateCalls++);
```

### Test Data Generation

**TestHelper factory methods:**
```csharp
internal static List<VehicleModel> GenerateVehicles(int count)
{
    List<VehicleModel> vehicles = [];
    for (int i = 0; i < count; i++)
    {
        vehicles.Add(GenerateVehicle());
    }
    return vehicles;
}
```

**Faker for German locale:**
```csharp
private static readonly Faker Faker = new("de");
var name = Faker.Random.AlphaNumeric(20);
var phone = Faker.Phone.PhoneNumber();
var guid = Faker.Random.Guid().ToString();
```

---

## 5. File Organization

### Directory Structure

**Main Application (TetraControl2Connect):**
```
TetraControl2Connect/
├── Program.cs                    # Startup and DI configuration
├── Constants.cs                  # Static constants and version info
├── Agent.cs                      # BackgroundService coordinator
├── Services/                     # Domain service interfaces and implementations
│   ├── IVehicleService.cs
│   ├── VehicleService.cs
│   ├── IConnectApiService.cs
│   ├── ConnectApiService.cs
│   ├── ITetraControlClient.cs
│   ├── TetraControlClient.cs
│   ├── IUserService.cs
│   ├── UserService.cs
│   ├── ISDSService.cs
│   ├── SDSService.cs
│   ├── ISitesService.cs
│   ├── SitesService.cs
│   ├── ISirenService.cs
│   └── SirenService.cs
├── Models/
│   ├── Connect/                  # Connect API DTOs
│   │   ├── VehicleModel.cs
│   │   ├── UserModel.cs
│   │   ├── OperationModel.cs
│   │   ├── StatusModel.cs
│   │   ├── StatusPositionModel.cs
│   │   ├── PositionModel.cs
│   │   ├── DefectReportModel.cs
│   │   └── ... (other Connect models)
│   └── TetraControl/             # TetraControl WebSocket DTOs
│       ├── TetraControlDto.cs
│       ├── MessageTypes.cs
│       ├── SdsTypes.cs
│       └── StatusType.cs
├── Extensions/                   # Custom extension methods
│   ├── ObservableExtensions.cs
│   ├── OperationModelExtensions.cs
│   ├── TetraControlDtoExtensions.cs
│   └── StringExtensions.cs
├── Converters/                   # JSON converters
│   └── UnixEpochDateTimeConverter.cs
└── Assets/
    └── icon.ico
```

**Shared Project (TetraControl2Connect.Shared):**
```
TetraControl2Connect.Shared/
└── Options/
    ├── ProgramOptions.cs         # Feature flags and timeouts
    ├── TetraControlOptions.cs    # WebSocket config
    ├── ConnectOptions.cs         # Connect API config
    ├── StatusOptions.cs          # Status mapping
    └── Models/
        ├── Site.cs               # Multi-site configuration
        ├── Siren.cs
        ├── SubnetAddress.cs
        └── ... (other models)
```

**Test Project (TetraControl2Connect.Test):**
```
TetraControl2Connect.Test/
├── Services/                     # Service unit tests
│   ├── VehicleService.Test.cs
│   ├── UserService.Test.cs
│   ├── SDSService.Test.cs
│   └── SirenService.Test.cs
├── Extensions/                   # Extension method tests
│   ├── TetraControlDtoExtensions.Test.cs
│   └── StringExtensions.Test.cs
└── Helper/
    └── TestHelper.cs             # Test data generation
```

### File Naming Rules

- **Classes/Interfaces:** `ClassName.cs` or `IInterfaceName.cs`
- **Test Files:** `ClassNameTest.cs` (not `ClassNameTests.cs`)
- **Extension Classes:** `TargetTypeExtensions.cs` (e.g., `ObservableExtensions.cs`)
- **Converters:** `TypeNameConverter.cs`

### One Class Per File

Each file contains **exactly one public type** (class, interface, record, enum). Helper types can be internal.

---

## 6. Key Technical Details

### Reactive Patterns - System.Reactive (Rx.NET)

The application uses **System.Reactive (IObservable)** for event streaming and subscriptions.

**Observable Subjects for Event Broadcasting:**
```csharp
private readonly Subject<TetraControlDto> _statusSubject = new();
private readonly Subject<TetraControlDto> _positionSubject = new();
private readonly Subject<bool> _connectionSubject = new();

public IObservable<TetraControlDto> StatusReceived => _statusSubject.AsObservable();
```

**Periodic Refresh with Observable.Interval:**
```csharp
_refreshSubscription = Observable
    .Interval(TimeSpan.FromHours(6))
    .SubscribeAsyncSafe(async _ =>
    {
        _log.LogInformation("Refreshing vehicles after 6 hours...");
        await LoadOrRefreshVehiclesAsync();
    },
    e => _log.LogError(e, "Failed to refresh vehicles."),
    () => _log.LogDebug("Vehicle refresh subscription completed."));
```

**Custom SubscribeAsyncSafe Extension:**
```csharp
public static IDisposable SubscribeAsyncSafe<T>(
    this IObservable<T> source,
    Func<T, Task> onNextAsync,
    Action<Exception> onError,
    Action onCompleted)
{
    return source
        .Select(arg => Observable.FromAsync(async () =>
        {
            try { await onNextAsync(arg).ConfigureAwait(false); }
            catch (Exception ex) { onError(ex); }
        }))
        .Concat()
        .Subscribe(_ => { }, onError, onCompleted);
}
```

**LINQ Operators Used:**
- `.Interval()` - periodic emissions
- `.Select()` - transform values
- `.Concat()` - sequential subscriptions
- `.Subscribe()` - subscribe with handlers
- `.AsObservable()` - expose subject as IObservable

### WebSocket Communication

**WebSocket Client Setup:**
- Uses `Websocket.Client` NuGet package
- Located in `TetraControlClient` service (implements `ITetraControlClient`)
- Automatically reconnects with configurable timeout

**Initialization:**
```csharp
var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
{
    Options =
    {
        KeepAliveInterval = TimeSpan.FromSeconds(30),
        Credentials = new NetworkCredential(
            userName: _tetraControlOptions.TetraControlUsername,
            password: _tetraControlOptions.TetraControlPassword),
    }
});

_wsClient = new(_tetraControlOptions.WebSocketUri, factory)
{
    ReconnectTimeout = TimeSpan.FromMinutes(_programOptions.WebSocketReconnectTimeoutMinutes),
    IsReconnectionEnabled = true
};
```

**Message Routing:**
WebSocket messages from TetraControl are deserialized to `TetraControlDto` and routed to appropriate `Subject<T>`:
- **Status messages** → `_statusSubject`
- **Position messages** → `_positionSubject`
- **SDS messages** → `_sDSSubject`
- **Siren status** → `_sirenStatusSubject`

**Subscription Pattern:**
```csharp
_tetraControlClient.PositionReceived
    .SubscribeAsyncSafe(HandleVehiclePosition, ...)
```

### REST API Integration

**ConnectApiService Pattern:**
```csharp
private async Task<T?> CallService<T>(
    Func<HttpClient, Task<HttpResponseMessage>> action,
    string accessToken,
    CancellationToken cancellationToken)
{
    using var httpClient = _httpClientFactory.CreateClient(nameof(IConnectApiService));
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    
    using var response = await action(httpClient);
    
    if (response.IsSuccessStatusCode)
    {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrEmpty(responseContent))
        {
            return JsonSerializer.Deserialize<T>(responseContent) ?? default;
        }
    }
    return default;
}
```

**Helper Methods:**
```csharp
private Task<T?> Get<T>(string url, string accessToken, CancellationToken ct)
    => CallService<T>(client => client.GetAsync(url, ct), accessToken, ct);

private Task<T?> Post<T>(string url, T data, string accessToken, CancellationToken ct)
    => CallService<T>(client => client.PostAsync(url, new StringContent(JsonSerializer.Serialize(data), ...)), accessToken, ct);
```

**Retry Policy (Polly):**
```csharp
private static AsyncRetryPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .OrResult(r => r.StatusCode == HttpStatusCode.TooManyRequests)
        .OrResult(r => r.StatusCode == HttpStatusCode.Conflict)
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(retryAttempt, 2)));
}
```

### Logging & Structured Logging

**Serilog Configuration:**
- Logs to file: `logs\TetraControl2Connect_Log_YYYY-MM-DD.txt`
- Logs to console and debug output
- Daily rolling files with 31-day retention
- Destructuring enabled with depth/string length limits
- Separate logger for siren events

**Usage:**
```csharp
_log.LogDebug($"Initializing {nameof(VehicleService)}.");
_log.LogInformation($"Sent position update for vehicle '{vehicle.Description}'");
_log.LogWarning($"No vehicle with ISSI '{issi}' found.");
_log.LogError(ex, "Error happened on CallService.");
```

**Log Masking:**
- Use `[LogMasked]` attribute from Destructurama for sensitive fields
- Example: `[LogMasked] public required string Key { get; set; }`

### Configuration (appsettings.json)

**Option Classes with Sections:**
- `ProgramOptions` - Feature flags, timeouts, status suppression
- `TetraControlOptions` - WebSocket URI, credentials
- `ConnectOptions` - Sites with access tokens
- `StatusOptions` - Status type mappings
- `PatternOptions` - Pattern matching for operations
- `SeverityOptions` - Severity level mappings
- `SirenCalloutOptions` - Siren configuration
- `SirenStatusOptions` - Siren status mappings

**Record-based Options:**
```csharp
public record ProgramOptions
{
    public const string SectionName = nameof(ProgramOptions);
    public bool SendVehicleStatus { get; set; } = true;
    public int WebSocketReconnectTimeoutMinutes { get; set; }
    // ...
}
```

---

## Development Guidelines

### When Adding New Services

1. Create interface `INewService.cs` in `Services/` folder
2. Implement `NewService.cs` with:
   - Primary constructor with DI parameters
   - Null-throw validation for all parameters
   - Private readonly fields for dependencies
   - `async Task Initialize()` if state initialization needed
   - `void Dispose()` if resources need cleanup
3. Register in `Program.cs`: `.AddSingleton<INewService, NewService>()`
4. Create corresponding test class: `NewServiceTest.cs`

### When Adding New Models

1. Use `public record` keyword for DTOs
2. Location depends on source:
   - `Models/Connect/` for Connect API DTOs
   - `Models/TetraControl/` for TETRA system DTOs
3. Initialize properties with defaults: `string = string.Empty`, `List<T> = []`
4. Add `[JsonPropertyName(...)]` for JSON deserialization
5. Add `[LogMasked]` for sensitive data

### When Creating Observable Subscriptions

1. Store `IDisposable?` subscription reference
2. Subscribe using `.SubscribeAsyncSafe()` for async operations
3. Dispose in `Dispose()` method: `_subscription?.Dispose()`
4. Always provide error and completion handlers

### Code Quality Standards

- **Nullable reference types:** Always enabled, use `?` for nullable types
- **No nulls where not needed:** Use empty collections `[]` instead of null
- **Logging level:** Debug for initialization, Info for operations, Warning for unexpected states
- **Exceptions:** Throw with descriptive messages, use ArgumentNullException for parameters
- **Thread safety:** Use `ConcurrentDictionary` for shared mutable state
- **Async/await:** Use `.ConfigureAwait(false)` except where context matters

---

## Project Technical Stack

- **Target Framework:** .NET 10.0
- **Language Version:** C# 13
- **Current Version:** 2.9.1
- **Build Type:** Console Application (Exe)
- **Nullable Reference Types:** Enabled
- **Implicit Usings:** Enabled

**Key Dependencies:**
- `Microsoft.Extensions.Hosting` - Host/DI/Configuration
- `Microsoft.Extensions.Logging` - Structured logging
- `Serilog` - Log implementation
- `System.Reactive` - Rx.NET for observables
- `Websocket.Client` - WebSocket connectivity
- `Polly` - Retry policies
- `Destructurama` - Object destructuring for logging

**Test Dependencies:**
- `xUnit` - Test framework
- `Moq` - Mocking library
- `FluentAssertions` - Fluent test assertions
- `Bogus` - Fake data generation

---

## Summary Checklist for Contributors

✓ Use primary constructors for services  
✓ Implement `IDisposable` for services managing resources  
✓ Register new services in `Program.cs`  
✓ Write tests alongside features  
✓ Use `ConcurrentDictionary` for thread-safe state  
✓ Use Observable/Rx patterns for event streams  
✓ Add `[LogMasked]` to sensitive properties  
✓ Use `.ConfigureAwait(false)` in library code  
✓ Validate constructor parameters with `?? throw new ArgumentNullException()`  
✓ Empty collections with `[]` not null  
✓ String interpolation with `$"..."` for logging  

---

## 10. Admin UI (Nuxt 3)

### Tech Stack
- **Nuxt 3** (v4.x) with SSG (Static Site Generation)
- **@nuxt/ui** v4 component library (Tailwind CSS-based)
- **pnpm** package manager
- **TypeScript** throughout

### Project Structure
```
TetraControl2Connect/AdminUI/
├── app.vue                     # Root component
├── nuxt.config.ts              # Nuxt configuration (SSG, proxy, modules)
├── composables/
│   └── useSettings.ts          # Settings API composable (fetch/update/toast)
├── layouts/
│   └── default.vue             # Sidebar navigation layout
├── pages/
│   ├── index.vue               # Dashboard overview
│   ├── live.vue                # SignalR live message view
│   └── settings/
│       ├── program.vue         # ProgramOptions editor
│       ├── tetracontrol.vue    # TetraControlOptions editor
│       ├── connect.vue         # ConnectOptions editor (Sites/Subnets/Sirens)
│       ├── status.vue          # StatusOptions editor
│       ├── pattern.vue         # PatternOptions editor
│       ├── severity.vue        # SeverityOptions editor
│       ├── siren-callout.vue   # SirenCalloutOptions editor
│       └── siren-status.vue    # SirenStatusOptions editor
└── package.json
```

### Conventions
- **Language:** All UI labels and messages in German
- **API calls:** Use `$fetch` via `useSettings()` composable
- **Components:** Nuxt UI v4 components (`UButton`, `UInput`, `USwitch`, `UCard`, `UNavigationMenu`, etc.)
- **Notifications:** Toast messages via `useToast()` for success/error feedback
- **Dev proxy:** `/api/**` → `http://localhost:5050/api/**` during development

### API Integration
The Admin UI talks to the .NET Web API at `/api/settings`:
- `GET /api/settings` — List all settings sections
- `GET /api/settings/{sectionName}` — Get one section's JSON
- `PUT /api/settings/{sectionName}` — Update one section
- `POST /api/settings/seed` — Re-seed from appsettings.json

### Build Commands
```bash
cd TetraControl2Connect/AdminUI
pnpm install              # Install dependencies
pnpm run dev              # Dev server on :3000 (proxies to .NET on :5050)
pnpm run generate         # SSG build → .output/public/
```


