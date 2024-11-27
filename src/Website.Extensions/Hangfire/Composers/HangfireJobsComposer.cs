using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SoleMates.Website.Extensions.Sync.HangfireJobs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SoleMates.Website.Extensions.Hangfire.Composers;
public sealed class HangfireJobsComposer : IComposer {
    public void Compose(IUmbracoBuilder builder) {
        builder.Components().Append<ScheduleHangfireComponent>();
    }

    public class ScheduleHangfireComponent : IComponent {
        private readonly IUmbracoContextFactory _umbracoContextFactory;
        private readonly IScopeProvider _scopeProvider;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ScheduleHangfireComponent(IUmbracoContextFactory umbracoContextFactory, IScopeProvider scopeProvider, IWebHostEnvironment hostingEnvironment) {
            _umbracoContextFactory = umbracoContextFactory;
            _scopeProvider = scopeProvider;
            _hostingEnvironment = hostingEnvironment;
        }

        public void Initialize() {
            BackgroundJob.Enqueue<ScheduleHangfireComponent>(a => EnqueueIt(null, "Test queue"));

            if (_hostingEnvironment.IsProduction()) {
                RecurringJob.AddOrUpdate<FetchJobs>("Sync Everything", (job) => job.SyncEverythingFromSourceJob(), Cron.Daily);
                RecurringJob.AddOrUpdate<FetchJobs>("Sync Stock", (job) => job.SyncStockFromSourceJob(), Cron.Hourly());
            } else {
                RecurringJob.AddOrUpdate<FetchJobs>("Sync Everything", (job) => job.SyncEverythingFromSourceJob(), Cron.Never); //Manual triggers when developing.
                RecurringJob.AddOrUpdate<FetchJobs>("Sync Stock", (job) => job.SyncStockFromSourceJob(), Cron.Never);
            }
        }

        public void Terminate() {
        }

        public void DoRecurringJob(PerformContext? context) {
            using UmbracoContextReference contextReference = _umbracoContextFactory.EnsureUmbracoContext();
            using IScope scope = _scopeProvider.CreateScope(autoComplete: true);

            context.WriteLine("Running: RecurringJob");
            IProgressBar progressBar = context.WriteProgressBar();
            int[] items = [2, 4, 6, 8, 10, 12, 14, 16, 18, 20];

            foreach (int item in items.WithProgress(progressBar, items.Length)) {
                context.WriteLine($"Number: {item}");
                Thread.Sleep(1000);
            }
        }

        public static void EnqueueIt(PerformContext? context, string test) {
            context.WriteLine($"Running: {test}");
            IProgressBar progressBar = context.WriteProgressBar();
            int[] items = [2, 4, 6, 8, 10, 12, 14, 16, 18, 20];

            foreach (int item in items.WithProgress(progressBar, items.Length)) {
                context.WriteLine($"Number: {item}");
                Thread.Sleep(1000);
            }
        }
    }
}
