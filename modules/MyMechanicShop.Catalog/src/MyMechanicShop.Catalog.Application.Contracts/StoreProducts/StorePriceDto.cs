using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using MyMechanicShop.SharedKernel.Enums;
using Volo.Abp.Validation.Localization;

namespace MyMechanicShop.Catalog.StoreProducts;

public class StorePriceDto : IValidatableObject
{
    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal PriceAmount { get; set; }

    public Currency PriceCurrency { get; set; } = Currency.Brl;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!PriceCurrency.IsAccepted())
        {
            var l = validationContext.GetRequiredService<IStringLocalizer<AbpValidationResource>>();
            yield return new ValidationResult(l["The {0} field is required.", nameof(PriceCurrency)],
                [nameof(PriceCurrency)]);
        }
    }
}
