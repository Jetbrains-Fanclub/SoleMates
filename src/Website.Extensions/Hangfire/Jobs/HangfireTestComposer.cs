using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using SoleMates.Website.Extensions.Fetch.HangfireJobs;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Scoping;

namespace SoleMates.Website.Extensions.Hangfire.Jobs;
public sealed class HangfireTestComposer : IComposer {
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
      RecurringJob.AddOrUpdate<ScheduleHangfireComponent>("DoRecurringJob", a => a.DoRecurringJob(null), Cron.Never());
      BackgroundJob.Enqueue<ScheduleHangfireComponent>(a => a.EnqueueIt(null, "Test queue"));

      if (_hostingEnvironment.IsProduction()) {
        RecurringJob.AddOrUpdate<FetchJob>("Fetch", (job) => job.CreateNodesFromSourceJob(), Cron.Daily);
      } else {
        RecurringJob.AddOrUpdate<FetchJob>("Fetch", (job) => job.CreateNodesFromSourceJob(), Cron.Never); //Manual triggers when developing.
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

    public void EnqueueIt(PerformContext? context, string test) {
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
