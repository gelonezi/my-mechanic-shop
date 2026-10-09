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

/// <summary>
/// Primitive input: the application service builds the value objects. Everything a user can get
/// wrong is rejected here, as a localized 400, before the value objects' guards (a 500) could fire.
/// </summary>
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

    /// <summary>Rules DataAnnotations cannot express. ABP runs this with the other validations.</summary>
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
