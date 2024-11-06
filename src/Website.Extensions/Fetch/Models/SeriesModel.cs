namespace Website.Extensions.Fetch.Models;
public record SeriesModel(string Name, string Brand, int Price, List<SizeModel> Sizes);
public record SizeModel(string SKU, int Size, int Stock);
