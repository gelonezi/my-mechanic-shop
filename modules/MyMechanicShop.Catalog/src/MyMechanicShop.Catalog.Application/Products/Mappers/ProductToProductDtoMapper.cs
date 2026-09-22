using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace MyMechanicShop.Catalog.Products.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProductToProductDtoMapper : MapperBase<Product, ProductDto>
{
    public override partial ProductDto Map(Product source);

    public override partial void Map(Product source, ProductDto destination);
}
