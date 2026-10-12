using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MyMechanicShop.SharedKernel.Enums;
using MyMechanicShop.SharedKernel.Localization;
using MyMechanicShop.SharedKernel.ValueObjects;
using Volo.Abp.Validation.Localization;

namespace MyMechanicShop.Catalog.Products;

public class CreateUpdateProductDto : IValidatableObject
{
    [Required]
    [StringLength(NameConsts.MaxLength)]
    public string Name { get; set; } = null!;

    [StringLength(NameConsts.MaxLength)]
    public string? Brand { get; set; }

    [StringLength(EanConsts.MaxLength)]
    public string? Ean { get; set; }

    [StringLength(DescriptionConsts.MaxLength)]
    public string? Description { get; set; }

    public ProductUnit Unit { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Unit == ProductUnit.Undefined || !Enum.IsDefined(Unit))
        {
            var l = validationContext.GetRequiredService<IStringLocalizer<AbpValidationResource>>();
            yield return new ValidationResult(l["The {0} field is required.", nameof(Unit)], [nameof(Unit)]);
        }

        if (!string.IsNullOrWhiteSpace(Ean) && !EanValidator.IsValid(Ean.Trim()))
        {
            var l = validationContext.GetRequiredService<IStringLocalizer<SharedKernelResource>>();
            yield return new ValidationResult(l["InvalidEan", nameof(Ean)], [nameof(Ean)]);
        }
    }
}
