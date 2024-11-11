using SoleMates.Website.Extensions.Sync.Adapters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SoleMates.Website.Extensions.Sync.HangfireJobs;
public class SyncJobs {
  private readonly IScopeProvider _scopeProvider;
  private readonly UmbracoAdapter _umbracoAdapter;
  private readonly IUmbracoContextFactory _umbracoContextFactory;

  public SyncJobs(IUmbracoContextFactory umbracoContextFactory, IScopeProvider scopeProvider, UmbracoAdapter umbracoAdapter) {
    _scopeProvider = scopeProvider;
    _umbracoAdapter = umbracoAdapter;
    _umbracoContextFactory = umbracoContextFactory;
  }

  public async Task SyncEverythingFromSourceJob() {
    //TODO: Exception handling and logging stuff
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _umbracoAdapter.SyncEverythingFromRest();
  }

  public async Task SyncStockFromSourceJob() {
    //TODO: Exception handling and logging stuff
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _umbracoAdapter.SyncStockFromRest();
  }
}
