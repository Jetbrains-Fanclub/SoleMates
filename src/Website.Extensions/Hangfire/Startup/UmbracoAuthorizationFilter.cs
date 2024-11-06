using Hangfire.Dashboard;

namespace Website.Extensions.Hangfire.Startup; 
public sealed class UmbracoAuthorizationFilter : IDashboardAuthorizationFilter {
  public bool Authorize( DashboardContext context ) {
    return true;
  }
}