using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace SoleMates.Website.Extensions.Hangfire.Extensions;
public static class HangfireExtensions {
  public static void PurgeJobs(this JobStorage? jobStorage) {
    if (jobStorage == null) {
      return;
    }

    IMonitoringApi? monitor = jobStorage.GetMonitoringApi();
    if (monitor == null) {
      return;
    }

    List<string> toDelete = [];

    foreach (QueueWithTopEnqueuedJobsDto queue in monitor.Queues()) {
      for (int i = 0; i < Math.Ceiling(queue.Length / 1000d); i++) {
        monitor.EnqueuedJobs(queue.Name, 1000 * i, 1000)
            .ForEach(x => toDelete.Add(x.Key));
      }

      if (queue.Fetched.HasValue) {
        for (int i = 0; i < Math.Ceiling(queue.Fetched.Value / 1000d); i++) {
          monitor.FetchedJobs(queue.Name, 1000 * i, 1000).ForEach(x => toDelete.Add(x.Key));
        }
      }
    }

    foreach (string jobId in toDelete) {
      BackgroundJob.Delete(jobId);
    }
  }

  public static void PurgeRecurringJobs(this JobStorage? jobStorage) {
    if (jobStorage == null) {
      return;
    }

    using IStorageConnection connection = jobStorage.GetConnection();

    foreach (RecurringJobDto recurringJob in connection.GetRecurringJobs()) {
      RecurringJob.RemoveIfExists(recurringJob.Id);
    }
  }
}
