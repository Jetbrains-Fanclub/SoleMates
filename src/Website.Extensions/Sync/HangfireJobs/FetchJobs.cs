using Microsoft.Extensions.Logging;
using SoleMates.Website.Extensions.Sync.Adapters;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SoleMates.Website.Extensions.Sync.HangfireJobs;
/// <summary> The class that contains all the HangFire jobs related to Syncing Umbraco.</summary>
public class FetchJobs {
  private readonly IScopeProvider _scopeProvider;
  private readonly UmbracoAdapter _umbracoAdapter;
  private readonly ILogger<FetchJobs> _logger;
  private readonly IUmbracoContextFactory _umbracoContextFactory;

  public FetchJobs(IUmbracoContextFactory umbracoContextFactory, IScopeProvider scopeProvider, UmbracoAdapter umbracoAdapter, ILogger<FetchJobs> logger) {
    _scopeProvider = scopeProvider;
    _umbracoAdapter = umbracoAdapter;
    _logger = logger;
    _umbracoContextFactory = umbracoContextFactory;
  }

  /// <summary> A Hangfire job that syncronizes everything from the source. <br/>
  /// This includes creating new nodes and updating the stock amount. </summary>
  public async Task SyncEverythingFromSourceJob() {
    _logger.LogInformation("Starting Syncronization Job: Everything from source.");
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _umbracoAdapter.SyncEverythingFromRest();
    _logger.LogInformation("Successfully finished syncronizing everything from source.");
  }

  /// <summary> A Hangfire job that syncronizes the stock amount from the source. </summary>
  public async Task SyncStockFromSourceJob() {
    _logger.LogInformation("Starting Syncronization Job: Stock amount from source.");
    using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
    using IScope scope = _scopeProvider.CreateScope(autoComplete: true);
    await _umbracoAdapter.SyncStockFromRest();
    _logger.LogInformation("Successfully finished syncronizing stock amount from source.");
  }
}
