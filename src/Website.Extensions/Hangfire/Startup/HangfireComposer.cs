using Hangfire;
using Hangfire.Console;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SoleMates.Website.Extensions.Hangfire.Extensions;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Web.BackOffice.Authorization;
using Umbraco.Cms.Web.Common.ApplicationBuilder;
using Constants = Umbraco.Cms.Core.Constants;

namespace SoleMates.Website.Extensions.Hangfire.Startup;
public sealed class HangfireComposer : IComposer {
    public void Compose(IUmbracoBuilder builder) {
        // Configure Hangfire to use our current database and add the option to write console messages
        string? connectionString = builder.Config.GetConnectionString("localExternalDB");

        if (string.IsNullOrEmpty(connectionString)) {
            connectionString = builder.Config.GetConnectionString(Constants.System.UmbracoConnectionName);
        }

        if (!string.IsNullOrEmpty(connectionString)) {
            builder.Services.AddHangfire(configuration => {
                configuration
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseConsole()
                    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions {
                        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                        QueuePollInterval = TimeSpan.Zero,
                        UseRecommendedIsolationLevel = true,
                        DisableGlobalLocks = true,
                    });
            });

            // Run the required server so your queued jobs will get executed
            builder.Services.AddHangfireServer();

            AddAuthorizedUmbracoDashboard(builder);

            // For some reason we need to give it the connection string again, else we get this error:
            // https://discuss.hangfire.io/t/jobstorage-current-property-value-has-not-been-initialized/884
            JobStorage.Current = new SqlServerStorage(connectionString);
            JobStorage.Current.PurgeJobs();
            JobStorage.Current.PurgeRecurringJobs();

        }
    }

    private static void AddAuthorizedUmbracoDashboard(IUmbracoBuilder builder) {
        // Add a named policy to authorize requests to the dashboard
        builder.Services.AddAuthorizationBuilder().AddPolicy(HangfireConstants.HangfireDashboard, policy => {
            // We require a logged in backoffice user who has access to the settings section
            policy.AuthenticationSchemes.Add(Constants.Security.BackOfficeAuthenticationType);
            policy.Requirements.Add(new SectionRequirement(Constants.Applications.Settings));
        });

        // Add the dashboard and make sure it's authorized with the named policy above
        builder.Services.Configure<UmbracoPipelineOptions>(options => {
            options.AddFilter(new UmbracoPipelineFilter(HangfireConstants.HangfireDashboard) {
                Endpoints = app => app.UseEndpoints(endpoints => {
                    endpoints.MapHangfireDashboard(
                            pattern: "/umbraco/backoffice/hangfire",
                            options: new DashboardOptions() {
                                Authorization = new[] { new UmbracoAuthorizationFilter() }
                            }).RequireAuthorization(HangfireConstants.HangfireDashboard);
                }).UseHangfireDashboard()
            });
        });
    }
}
