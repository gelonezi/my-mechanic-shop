using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Localization;
using MyMechanicShop.Catalog.Permissions;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace MyMechanicShop.Catalog.Products;

/// <summary>
/// The global catalog, maintained by the host's system administrator (host-only permissions).
/// </summary>
public class ProductAppService
    : CrudAppService<Product, ProductDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateProductDto>,
        IProductAppService
{
    /// <summary>DTO property (as the client sends it in <c>sorting</c>) → entity path.</summary>
    private static readonly Dictionary<string, string> SortablePaths = new(StringComparer.OrdinalIgnoreCase)
    {
        [nameof(ProductDto.Name)] = $"{nameof(Product.Name)}.{nameof(NameVo.Value)}",
        [nameof(ProductDto.Brand)] = $"{nameof(Product.Brand)}.{nameof(NameVo.Value)}",
        [nameof(ProductDto.Ean)] = $"{nameof(Product.Ean)}.{nameof(EanVo.Value)}",
        [nameof(ProductDto.Description)] = $"{nameof(Product.Description)}.{nameof(DescriptionVo.Value)}",
        [nameof(ProductDto.Unit)] = nameof(Product.Unit),
    };

    public ProductAppService(IRepository<Product, Guid> repository)
        : base(repository)
    {
        LocalizationResource = typeof(CatalogResource);
        ObjectMapperContext = typeof(CatalogApplicationModule);

        GetPolicyName = CatalogPermissions.Products.Default;
        GetListPolicyName = CatalogPermissions.Products.Default;
        CreatePolicyName = CatalogPermissions.Products.Create;
        UpdatePolicyName = CatalogPermissions.Products.Edit;
        DeletePolicyName = CatalogPermissions.Products.Delete;
    }

    /// <summary>Builds the value objects; the input was already validated by the DTO.</summary>
    protected override Task<Product> MapToEntityAsync(CreateUpdateProductDto createInput)
    {
        var product = new Product(GuidGenerator.Create(), NameVo.Create(createInput.Name), createInput.Unit);
        SetOptionalFields(product, createInput);
        return Task.FromResult(product);
    }

    protected override Task MapToEntityAsync(CreateUpdateProductDto updateInput, Product entity)
    {
        entity.SetName(NameVo.Create(updateInput.Name)).SetUnit(updateInput.Unit);
        SetOptionalFields(entity, updateInput);
        return Task.CompletedTask;
    }

    protected virtual void SetOptionalFields(Product product, CreateUpdateProductDto input)
    {
        product
            .SetBrand(string.IsNullOrWhiteSpace(input.Brand) ? null : NameVo.Create(input.Brand))
            .SetEan(string.IsNullOrWhiteSpace(input.Ean) ? null : EanVo.Create(input.Ean))
            .SetDescription(string.IsNullOrWhiteSpace(input.Description) ? null : DescriptionVo.Create(input.Description));
    }

    /// <summary>
    /// The client sorts by DTO properties (<c>name desc</c>), but the entity's are value objects:
    /// <c>OrderBy("Name")</c> would order by an object, which EF cannot translate.
    /// </summary>
    protected override IQueryable<Product> ApplySorting(IQueryable<Product> query, PagedAndSortedResultRequestDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Sorting))
        {
            return base.ApplySorting(query, input);
        }

        var sorting = string.Join(", ", input.Sorting
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(TranslateSortingPart));

        return query.OrderBy(sorting);
    }

    private static string TranslateSortingPart(string part)
    {
        var tokens = part.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var path = SortablePaths.GetValueOrDefault(tokens[0], tokens[0]);
        return tokens.Length > 1 ? $"{path} {tokens[1]}" : path;
    }
}
