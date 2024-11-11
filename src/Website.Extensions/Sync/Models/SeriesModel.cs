namespace SoleMates.Website.Extensions.Fetch.Models;
public record SeriesModel(string Name, string Brand, decimal Price, List<SizeModel> Sizes, int ID);
public record SizeModel(string SKU, int Size, int Stock);
