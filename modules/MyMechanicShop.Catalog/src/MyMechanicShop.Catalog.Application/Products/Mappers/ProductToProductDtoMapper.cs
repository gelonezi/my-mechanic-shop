using MyMechanicShop.SharedKernel.ValueObjects;
using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace MyMechanicShop.Catalog.Products.Mappers;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class ProductToProductDtoMapper : MapperBase<Product, ProductDto>
{
    public override partial ProductDto Map(Product source);

    public override partial void Map(Product source, ProductDto destination);

    // Value object → primitive. Mapperly uses these for the nullable ones too, mapping null to null.
    private static string MapName(NameVo name) => name.Value;

    private static string MapEan(EanVo ean) => ean.Value;

    private static string MapDescription(DescriptionVo description) => description.Value;
}
