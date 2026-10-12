---
paths:
  - "src/**/*.cs"
  - "test/**/*.cs"
---

# ABP Layered Application Template

> **Docs**: https://abp.io/docs/latest/solution-templates/layered-web-application

This template follows **Domain-Driven Design (DDD)** principles with strict layer separation.

## Solution Structure

```
MyMechanicShop/
├── src/
│   ├── MyMechanicShop.Domain.Shared/        # Constants, enums, localization, ETOs
│   ├── MyMechanicShop.Domain/               # Entities, repository interfaces, domain services
│   ├── MyMechanicShop.Application.Contracts/ # DTOs, service interfaces
│   ├── MyMechanicShop.Application/          # Application service implementations
│   ├── MyMechanicShop.EntityFrameworkCore/  # Host DbContext + its migrations (Migrations/ABP)
│   ├── MyMechanicShop.HttpApi/              # REST API controllers (optional)
│   ├── MyMechanicShop.HttpApi.Client/       # Client proxies for remote calls
│   ├── MyMechanicShop.HttpApi.Host/         # The API (`just api`)
│   └── MyMechanicShop.DbMigrator/           # Applies every database's migrations + seeds
├── modules/                                 # Local ABP modules (own projects, database, Angular library)
├── angular/                                 # Angular SPA (`just web`)
└── test/
    ├── MyMechanicShop.Domain.Tests/
    ├── MyMechanicShop.Application.Tests/
    └── MyMechanicShop.EntityFrameworkCore.Tests/
```

## Layer Responsibilities

| Layer | Responsibility | References |
|-------|---------------|------------|
| **Domain.Shared** | Constants, enums, localization keys, ETOs | Nothing |
| **Domain** | Entities, aggregate roots, domain services, repository interfaces | Domain.Shared |
| **Application.Contracts** | DTOs, service interfaces | Domain.Shared |
| **Application** | Use case orchestration, mapping | Domain, Application.Contracts |
| **EntityFrameworkCore** | DbContext, repository implementations, migrations | Domain |
| **HttpApi** | REST controllers (if not using Auto API) | Application.Contracts |
| **angular** | Angular SPA components, services | HttpApi.Client (via proxies) |
| **DbMigrator** | Database migration + seeding console app | EntityFrameworkCore |

## Key Differences from Other Templates

| Layered (This) | Single-Layer | Microservice |
|---------------|--------------|--------------|
| Strict DDD layers | Single project | Single project per service |
| DTOs in Application.Contracts | DTOs in Services folder | DTOs in Contracts project |
| Custom repository interfaces in Domain | Generic repositories only | Generic repositories only |
| Multiple module classes | Single module class | Single module class per service |
| DbMigrator project | Migration in host | Migration per service |

## Adding a New Feature

### 1. Entity in Domain
```csharp
public class Book : AuditedAggregateRoot<Guid>
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }

    protected Book() { } // For ORM

    public Book(Guid id, string name, decimal price) : base(id)
    {
        SetName(name);
        SetPrice(price);
    }

    public void SetName(string name)
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: BookConsts.MaxNameLength);
    }

    public void SetPrice(decimal price)
    {
        Price = Check.Range(price, nameof(price), 0, 9999);
    }
}
```

### 2. Constants in Domain.Shared
```csharp
public static class BookConsts
{
    public const int MaxNameLength = 128;
}
```

### 3. Repository Interface in Domain (if custom queries needed)
```csharp
public interface IBookRepository : IRepository<Book, Guid>
{
    Task<Book> FindByNameAsync(string name);
}
```

### 4. DTOs in Application.Contracts
```csharp
public class BookDto : EntityDto<Guid>
{
    public string Name { get; set; }
    public decimal Price { get; set; }
}

public class CreateBookDto
{
    [Required]
    [StringLength(BookConsts.MaxNameLength)]
    public string Name { get; set; }

    [Range(0, 9999)]
    public decimal Price { get; set; }
}
```

### 5. Service Interface in Application.Contracts
```csharp
public interface IBookAppService : IApplicationService
{
    Task<BookDto> GetAsync(Guid id);
    Task<PagedResultDto<BookDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<BookDto> CreateAsync(CreateBookDto input);
}
```

### 6. Service Implementation in Application
```csharp
public class BookAppService : ApplicationService, IBookAppService
{
    private readonly IRepository<Book, Guid> _bookRepository;
    private readonly BookMapper _bookMapper;

    public BookAppService(IRepository<Book, Guid> bookRepository, BookMapper bookMapper)
    {
        _bookRepository = bookRepository;
        _bookMapper = bookMapper;
    }

    public async Task<BookDto> GetAsync(Guid id)
    {
        var book = await _bookRepository.GetAsync(id);
        return _bookMapper.MapToDto(book);
    }

    [Authorize(MyProjectPermissions.Books.Create)]
    public async Task<BookDto> CreateAsync(CreateBookDto input)
    {
        var book = new Book(GuidGenerator.Create(), input.Name, input.Price);
        await _bookRepository.InsertAsync(book);
        return _bookMapper.MapToDto(book);
    }
}
```

### 7. DbContext Configuration in EntityFrameworkCore
```csharp
public DbSet<Book> Books { get; set; }

// In OnModelCreating
builder.Entity<Book>(b =>
{
    b.ToTable(MyProjectConsts.DbTablePrefix + "Books", MyProjectConsts.DbSchema);
    b.ConfigureByConvention();
    b.Property(x => x.Name).IsRequired().HasMaxLength(BookConsts.MaxNameLength);
});
```

### 8. Add and apply the migration
```bash
cd src/MyMechanicShop.EntityFrameworkCore
dotnet ef migrations add Added_Book --context MyMechanicShopDbContext --output-dir Migrations/ABP
```

Then `just api`: in Development the host applies it before it listens. A module's entity
gets its migration in the module's own EF project instead (the `ef-migration` skill).

## DbMigrator

Console application for:
- Applying every database's migrations (host and modules)
- Seeding initial data (admin user, permissions, etc.)

In Development the host runs the DbMigrator's migration service on startup, so `just api` is
enough. Elsewhere, run it before the host, **from its own folder** (it is its content root;
`dotnet run --project src/…` finds no `appsettings.json`):
```bash
cd src/MyMechanicShop.DbMigrator
dotnet run
```

## Best Practices

1. **Respect layer boundaries** - Never reference EntityFrameworkCore from Application
2. **Rich domain model** - Encapsulate business logic in entities
3. **Repository per aggregate** - One repository per aggregate root only
4. **DTOs at boundaries** - Never expose entities outside Application layer
5. **Use DbMigrator** - For consistent database setup across environments
6. **Auto API Controllers** - Let ABP generate controllers from app services
7. **Mapperly for mapping** - Use compile-time mapper for performance
