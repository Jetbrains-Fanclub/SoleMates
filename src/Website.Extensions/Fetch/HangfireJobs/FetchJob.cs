using SoleMates.Website.Extensions.Fetch.Adapters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SoleMates.Website.Extensions.Fetch.HangfireJobs;
public class FetchJob {
  private readonly IScopeProvider _scopeProvider;
  private readonly UmbracoAdapter _umbracoAdapter;
  private readonly IUmbracoContextFactory _umbracoContextFactory;

  public FetchJob(IUmbracoContextFactory umbracoContextFactory, IScopeProvider scopeProvider, UmbracoAdapter umbracoAdapter) {
    _scopeProvider = scopeProvider;
    _umbracoAdapter = umbracoAdapter;
    _umbracoContextFactory = umbracoContextFactory;
  }

  public async Task CreateNodesFromSourceJob() {
    //TODO: Exception handling and logging stuff
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _umbracoAdapter.CreateNodesFromSource();
  }
}
