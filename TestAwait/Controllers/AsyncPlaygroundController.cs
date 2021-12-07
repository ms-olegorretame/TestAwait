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
        public async Task<LatencyReport> Get(bool awaitTrue, bool awaitFalse, bool discard, bool startNew, bool workItem)
        {
            var callStarted = DateTime.UtcNow;
            Task myTask = null;
            Console.WriteLine($"----------------------------------------------");
            if (awaitTrue)
            {
                Console.WriteLine($"Before await (true): {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
                await longRunning("await (true)").ConfigureAwait(true);
                Console.WriteLine($"After await (true): {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            }

            if (awaitFalse)
            {
                Console.WriteLine($"Before await (false): {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
                await longRunning("await (false)").ConfigureAwait(false);
                Console.WriteLine($"After await (false): {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            }

            if (discard)
            {
                Console.WriteLine($"Before discard: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
                _ = longRunning("discard");
                Console.WriteLine($"After discard: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            }

            if (startNew)
            {
                Console.WriteLine($"Before StartNew: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
                myTask = Task.Factory.StartNew(() => longRunning("StartNew"));
                Console.WriteLine($"myTask is: {myTask.Status}");
                Console.WriteLine($"After StartNew: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            }

            if (workItem)
            {
                Console.WriteLine($"Before QueueUserWorkItem: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
                ThreadPool.QueueUserWorkItem(new WaitCallback(state => longRunning("workitem")));
                Console.WriteLine($"After QueueUserWorkItem: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            }

            if (myTask is not null)
            {
                Console.WriteLine($"myTask is: {myTask.Status}");
            }

            Console.WriteLine($">>>>>>>> Time to return: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            var callEnded = DateTime.UtcNow;
            return new(callStarted, callEnded, (callEnded-callStarted).Ticks);

        }

      
        private async Task longRunning(string pattern)
        {
            Console.WriteLine($"[{pattern}] Before Sleep: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            Thread.Sleep(3000);
            Console.WriteLine($"[{pattern}] After Sleep: {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
            await Task.Delay(1000);
            Console.WriteLine($"[{pattern}] After await (inside): {DateTime.UtcNow} Thread: {Thread.CurrentThread.ManagedThreadId}");
        }
      
    }
}