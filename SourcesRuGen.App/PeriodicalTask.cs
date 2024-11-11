using System;
using System.Threading;
using System.Threading.Tasks;
using SourcesRuGen.Config;

namespace SourcesRuGenApp
{

    public class PeriodicTask
    {
        public static async Task Run(Action action, TimeSpan period, CancellationTokenSource cancellationToken, IConfiguration config)
        {
            Console.WriteLine("Start task " + cancellationToken.GetHashCode());
            
            while(!cancellationToken.IsCancellationRequested)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine("Cancel task " + cancellationToken.GetHashCode());
                    break;
                }

                if(config.TaskRunFirst)
                    action();
                
                await Task.Delay(period);
                
                if(!config.TaskRunFirst)
                    action();
            }
        }
    }

}