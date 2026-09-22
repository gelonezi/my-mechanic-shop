using MyMechanicShop.Samples;
using Xunit;

namespace MyMechanicShop.EntityFrameworkCore.Domains;

[Collection(MyMechanicShopTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<MyMechanicShopEntityFrameworkCoreTestModule>;
