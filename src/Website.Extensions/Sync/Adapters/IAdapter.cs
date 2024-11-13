namespace SoleMates.Website.Extensions.Sync.Adapters;
public interface IAdapter<T, K> {
  public Task<List<T>> FetchFromSource(K source);
}
