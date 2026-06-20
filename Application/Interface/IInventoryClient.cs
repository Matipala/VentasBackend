namespace VentasBackend.Application.Interface;

public interface IInventoryClient
{
    Task<StockValidationResponseDto> ValidateStockAsync(string companyCen, string warehouseCen, List<StockItemDto> items);
    Task<bool> ConsumeStockAsync(string companyCen, string warehouseCen, string referenceCen, string reason, List<StockItemDto> items);
    Task<List<SellableProductContractResponseDto>> GetSellableProductsAsync(string companyCen, string? search, string? categoryCen, string? warehouseCen, bool onlyAvailable);
    Task<Dictionary<string, string>> LookupProductNamesAsync(string companyCen, List<string> productCens);
}

public class StockItemDto
{
    public string ProductCen { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

public class StockValidationResponseDto
{
    public bool IsValid { get; set; }
    public List<StockRequirementDto> Requirements { get; set; } = new();
}

public class StockRequirementDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int MissingQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class SellableProductContractResponseDto
{
    public string ProductCen { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string CategoryCen { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal AvailableQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? StationCode { get; set; }
}
