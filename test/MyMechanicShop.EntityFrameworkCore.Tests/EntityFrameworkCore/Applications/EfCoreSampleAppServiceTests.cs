using MyMechanicShop.Samples;
using Xunit;

namespace MyMechanicShop.EntityFrameworkCore.Applications;

[Collection(MyMechanicShopTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<MyMechanicShopEntityFrameworkCoreTestModule>;
