namespace SoleMates.Website.Extensions.Sync.Adapters;
public interface ISourceAdapter<T, K, V> {
  public Task<List<T>> FetchFromSource(K source);
  public Task UpdateSource(K source, List<V> updateModels);
}
