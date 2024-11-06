using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Website.Extensions.Fetch.Models;
using Website.Extensions.Fetch.Services;

namespace Website.Extensions.Fetch.Composers;
public class FetchComposer : IComposer {
  public void Compose(IUmbracoBuilder builder) {
    //builder.Services.AddSingleton<FetchService>();
    //builder.Services.AddSingleton<FetchJobService>();
    //builder.Services.AddSingleton<RestAdapterService>();
    builder.Services.AddSingleton<LoggerService>();
    //builder.Services.AddSingleton<ConfigFinderService>();
    builder.Services.AddSingleton<NodeInitializerService>();
    builder.Services.AddSingleton<IAdapterService<SeriesModel, string>, RestAdapterService>();
  }
}
