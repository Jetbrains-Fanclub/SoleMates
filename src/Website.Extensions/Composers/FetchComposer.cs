using Microsoft.Extensions.DependencyInjection;
using SoleMates.Website.Extensions.Fetch.Adapters;
using SoleMates.Website.Extensions.Fetch.Models;
using SoleMates.Website.Extensions.Fetch.NodeHandlers;
using SoleMates.Website.Extensions.Fetch.Services;
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
