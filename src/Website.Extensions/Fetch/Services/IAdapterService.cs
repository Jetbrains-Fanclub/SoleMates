namespace Website.Extensions.Fetch.Services;
public interface IAdapterService<T, K> {
  public Task CreateNodesFromSource();
  public Task<List<T>> FetchFromSource(K source);
}
