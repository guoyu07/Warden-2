﻿using System;
using System.Threading.Tasks;
using NLog;
using Sentry.Core;
using Sentry.Watchers.Website;

namespace Sentry.Examples.Console
{
    class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly ISentry Sentry = ConfigureSentry();

        private static void Main(string[] args)
        {
            Task.WaitAll(Sentry.StartAsync());
        }

        private static ISentry ConfigureSentry()
        {
            var websiteWatcherConfiguration = WebsiteWatcherConfiguration
                .Create("httpstat.us website watcher")
                .WithUrl("http://httpstat.us/200")
                .Build();
            var websiteWatcher = new WebsiteWatcher(websiteWatcherConfiguration);
            var sentryConfiguration = SentryConfiguration
                .Create()
                .SetHooks(hooks =>
                {
                    hooks.OnError(exception => Logger.Error(exception));
                    hooks.OnIterationCompleted(iteration => OnIterationCompleted(iteration));
                })
                .AddWatcher(websiteWatcher, hooks =>
                {
                    hooks.OnStartAsync(check => WebsiteHookOnStartAsync(check));
                    hooks.OnFailureAsync(result => WebsiteHookOnFailureAsync(result));
                    hooks.OnSuccessAsync(result => WebsiteHookOnSuccessAsync(result));
                    hooks.OnCompletedAsync(result => WebsiteHookOnCompletedAsync(result));
                })
                .SetGlobalWatcherHooks(hooks =>
                {
                    hooks.OnStart(check => GlobalHookOnStart(check));
                    hooks.OnFailure(result => GlobalHookOnFailure(result));
                    hooks.OnSuccess(result => GlobalHookOnSuccess(result));
                    hooks.OnCompleted(result => GlobalHookOnCompleted(result));
                    hooks.OnError(exception => Logger.Error(exception));
                })
                .Build();

            return new Sentry(sentryConfiguration);
        }

        private static async Task WebsiteHookOnStartAsync(IWatcherCheck check)
        {
            Logger.Info($"Invoking the hook OnStartAsync() by watcher: '{check.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnSuccessAsync(ISentryCheckResult check)
        {
            var websiteWatcherCheckResult = (WebsiteWatcherCheckResult)check.WatcherCheckResult;
            Logger.Info($"Invoking the hook OnSuccessAsync() by watcher: '{websiteWatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnCompletedAsync(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the hook OnCompletedAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static async Task WebsiteHookOnFailureAsync(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the hook OnFailureAsync() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
            await Task.CompletedTask;
        }

        private static void GlobalHookOnStart(IWatcherCheck check)
        {
            Logger.Info($"Invoking the global hook OnStart() by watcher: '{check.WatcherName}'.");
        }

        private static void GlobalHookOnSuccess(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnSuccess() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnCompleted(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnCompleted() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void GlobalHookOnFailure(ISentryCheckResult check)
        {
            Logger.Info($"Invoking the global hook OnFailure() by watcher: '{check.WatcherCheckResult.WatcherName}'.");
        }

        private static void OnIterationCompleted(ISentryIteration sentryIteration)
        {
            var newLine = Environment.NewLine;
            Logger.Info($"Sentry iteration {sentryIteration.Ordinal} has completed.");
            foreach (var result in sentryIteration.Results)
            {
                Logger.Info($"Watcher: '{result.WatcherCheckResult.WatcherName}'{newLine}" +
                            $"Description: {result.WatcherCheckResult.Description}{newLine}" +
                            $"Is valid: {result.IsValid}{newLine}" +
                            $"Started at: {result.StartedAt}{newLine}" +
                            $"Completed at: {result.CompletedAt}{newLine}" +
                            $"Execution time: {result.ExecutionTime}{newLine}");
            }
        }
    }
}