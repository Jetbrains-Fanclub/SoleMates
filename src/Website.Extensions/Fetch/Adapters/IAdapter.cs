namespace SoleMates.Website.Extensions.Fetch.Adapters;
public interface IAdapter<T, K> {
  public Task<List<T>> FetchFromSource(K source);
}
