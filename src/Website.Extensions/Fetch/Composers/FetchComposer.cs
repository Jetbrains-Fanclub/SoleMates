using Microsoft.Extensions.DependencyInjection;
using SoleMates.Website.Extensions.Fetch.Adapters;
using SoleMates.Website.Extensions.Fetch.Models;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace SoleMates.Website.Extensions.Fetch.Composers;
public class FetchComposer : IComposer {
  public void Compose(IUmbracoBuilder builder) {
    builder.Services.AddSingleton<IAdapter<SeriesModel, string>, RestAdapter>();
    builder.Services.AddSingleton<UmbracoAdapter>();
  }
}
