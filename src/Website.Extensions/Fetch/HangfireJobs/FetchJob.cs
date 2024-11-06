using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;
using Website.Extensions.Fetch.Services;

namespace Website.Extensions.Fetch.HangfireJobs;
public class FetchJob<T, K> {
  private readonly IScopeProvider _scopeProvider;
  private readonly IAdapterService<T, K> _adapterService;
  private readonly IUmbracoContextFactory _umbracoContextFactory;

  public FetchJob(IAdapterService<T, K> adapterService, IUmbracoContextFactory umbracoContextFactory, IScopeProvider scopeProvider) {
    _scopeProvider = scopeProvider;
    _adapterService = adapterService;
    _umbracoContextFactory = umbracoContextFactory;
  }

  public async Task CreateNodesFromSourceJob() {
    //TODO: Exception handling and logging stuff
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _adapterService.CreateNodesFromSource();
  }
}
