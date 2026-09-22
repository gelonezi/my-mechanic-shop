using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace MyMechanicShop.Catalog.Products;

public class ProductAppService(IRepository<Product, Guid> repository)
    : CrudAppService<
            Product,
            ProductDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateProductDto>(repository),
        IProductAppService; 