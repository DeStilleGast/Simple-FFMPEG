using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf_simple_FFMPEG {
    static class Extensions {

        public static string ToTime(this double value) {
            TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)(value * 1000D));

            return ts.ToString(@"hh\:mm\:ss\.fff");
        }


        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// Source: https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
        /// immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsync(this Process process, CancellationToken cancellationToken = default(CancellationToken)) {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
