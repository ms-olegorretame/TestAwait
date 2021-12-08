using Microsoft.AspNetCore.Mvc;

namespace TestAwait.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AsyncPlaygroundController : ControllerBase
    {
        public record struct LatencyReport(DateTime CallStarted, DateTime CallEnded, long TimeSpent);

        private readonly ILogger<AsyncPlaygroundController> _logger;

        public AsyncPlaygroundController(ILogger<AsyncPlaygroundController> logger)
        {
            _logger = logger;
        }

        [HttpGet(Name = "GetTheTime")]
        public async Task<LatencyReport> Get(bool awaitTrue, bool awaitFalse, bool discard, bool startNew, bool taskRun, bool workItem)
        {
            var callStarted = DateTime.UtcNow;
            Task? newTask = null;
            Task? ranTask = null;
            LogMessage($"------------------------- {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}------------------");
            if (awaitTrue)
            {
                LogMessage($"Before await (true): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
                await LongRunning("await (true)").ConfigureAwait(true);
                LogMessage($"After await (true): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (awaitFalse)
            {
                LogMessage($"Before await (false): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
                await LongRunning("await (false)").ConfigureAwait(false);
                LogMessage($"After await (false): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (discard)
            {
                LogMessage($"Before discard: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
                _ = LongRunning("discard");
                LogMessage($"After discard: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (startNew)
            {
                LogMessage($"Before StartNew: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
                newTask = Task.Factory.StartNew(() => LongRunning("StartNew"));
                LogMessage($"newTask is: {newTask.Status} Thread: {Environment.CurrentManagedThreadId}");
                LogMessage($"After StartNew: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (taskRun)
            {
                LogMessage($"Before Task.Run: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
                ranTask = Task.Run(() => LongRunning("Task.Run"));
                LogMessage($"ranTask is: {ranTask.Status} Thread: {Environment.CurrentManagedThreadId}");
                LogMessage($"After Task.Run: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (workItem)
            {
                LogMessage($"Before QueueUserWorkItem: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                ThreadPool.QueueUserWorkItem(new (_ => LongRunning("workitem")));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                LogMessage($"After QueueUserWorkItem: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (newTask is not null)
            {
                LogMessage($"newTask is: {newTask.Status} Thread: {Environment.CurrentManagedThreadId}");
            }

            if (ranTask is not null)
            {
                LogMessage($"ranTask is: {ranTask.Status} Thread: {Environment.CurrentManagedThreadId}");
            }

            LogMessage($">>>>>>>> Time to return: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            var callEnded = DateTime.UtcNow;
            return new(callStarted, callEnded, (callEnded-callStarted).Ticks);

        }

        private void LogMessage(string message)
        {
            _logger.LogInformation(message);
        }

        private async Task LongRunning(string pattern)
        {
            LogMessage($"[{pattern}] Before Sleep: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            Thread.Sleep(3000);
            LogMessage($"[{pattern}] After Sleep: {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            LogMessage($"[{pattern}] Before await (inside): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
            await Task.Delay(1000);
            LogMessage($"[{pattern}] After await (inside): {DateTime.UtcNow} Thread: {Environment.CurrentManagedThreadId}");
        }
      
    }
}