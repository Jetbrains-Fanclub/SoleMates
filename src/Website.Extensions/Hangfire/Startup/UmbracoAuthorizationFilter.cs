using Hangfire.Dashboard;

namespace SoleMates.Website.Extensions.Hangfire.Startup;
public sealed class UmbracoAuthorizationFilter : IDashboardAuthorizationFilter {
    public bool Authorize(DashboardContext context) {
        return true;
    }
}
