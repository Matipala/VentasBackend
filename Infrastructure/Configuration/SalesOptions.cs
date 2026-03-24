namespace VentasBackend.Infrastructure.Configuration;

public class SalesOptions
{
    public const string SectionName = "Sales";
    public decimal GlobalTaxPercent { get; set; } = 13m;
}