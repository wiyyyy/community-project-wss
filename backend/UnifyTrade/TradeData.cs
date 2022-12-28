namespace UnifyTrade;

public class TradeData
{
    public string? Source { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public bool Direction { get; set; }
}

public class BinanceTradeRawData
{
    public byte[] RawData { get; set; }
}
public class BybitTradeRawData
{
    public byte[] RawData { get; set; }
}

public class HuobiTradeRawData
{
    public byte[] RawData { get; set; }
}