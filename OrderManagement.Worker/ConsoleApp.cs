using Microsoft.Extensions.Hosting;

namespace Subscriber;

public class ConsoleApp: IHostedService 
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Hello, World!");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}