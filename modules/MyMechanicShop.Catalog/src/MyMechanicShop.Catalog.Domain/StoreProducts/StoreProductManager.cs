using System;
using System.Threading.Tasks;
using MyMechanicShop.Catalog.Products;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace MyMechanicShop.Catalog.StoreProducts;

/// <summary>
/// Activates catalog products in the current shop, enforcing one <see cref="StoreProduct"/> per
/// shop and product.
/// </summary>
public class StoreProductManager : DomainService
{
    protected IRepository<StoreProduct, Guid> StoreProductRepository { get; }

    protected IRepository<Product, Guid> ProductRepository { get; }

    public StoreProductManager(
        IRepository<StoreProduct, Guid> storeProductRepository,
        IRepository<Product, Guid> productRepository)
    {
        StoreProductRepository = storeProductRepository;
        ProductRepository = productRepository;
    }

    /// <summary>
    /// Starts selling <paramref name="productId"/> in the current shop at <paramref name="price"/>.
    /// A product the shop had deactivated is reactivated with the new price.
    /// </summary>
    /// <exception cref="Volo.Abp.Domain.Entities.EntityNotFoundException">The catalog product does not exist.</exception>
    /// <exception cref="BusinessException"><see cref="CatalogErrorCodes.ProductAlreadyActive"/>.</exception>
    public virtual async Task<StoreProduct> ActivateAsync(Guid productId, MonetaryVo price)
    {
        await ProductRepository.EnsureExistsAsync(productId);

        // The tenant filter scopes this to the current shop.
        var existing = await StoreProductRepository.FindAsync(x => x.ProductId == productId);

        if (existing is null)
        {
            return await StoreProductRepository.InsertAsync(
                new StoreProduct(GuidGenerator.Create(), CurrentTenant.Id, productId, price),
                autoSave: true);
        }

        if (existing.IsActive)
        {
            throw new BusinessException(CatalogErrorCodes.ProductAlreadyActive)
                .WithData("ProductId", productId);
        }

        return await StoreProductRepository.UpdateAsync(existing.Reactivate(price), autoSave: true);
    }
}
