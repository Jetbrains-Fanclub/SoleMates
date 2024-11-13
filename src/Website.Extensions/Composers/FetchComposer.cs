using Microsoft.Extensions.DependencyInjection;
using SoleMates.Website.Extensions.Sync.Adapters;
using SoleMates.Website.Extensions.Sync.Models;
using SoleMates.Website.Extensions.Sync.NodeHandlers;
using SoleMates.Website.Extensions.Sync.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SoleMates.Website.Extensions.Composers;
public class FetchComposer : IComposer {
  public void Compose(IUmbracoBuilder builder) {
    builder.Services.AddSingleton<IAdapter<SeriesModel, string>, RestAdapter>();
    builder.Services.AddSingleton<UmbracoAdapter>();
    builder.Services.AddSingleton<HashingService>();
    builder.Services.AddSingleton<BaseNodesHandler>();
    builder.Services.AddSingleton<SeriesNodesHandler>();
    builder.Services.AddSingleton<SizeNodesHandler>();
  }
}
