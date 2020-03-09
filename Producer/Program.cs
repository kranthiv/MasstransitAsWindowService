using MassTransit;
using MasstransitHost;
using System.Threading.Tasks;

namespace Producer
{
    class Program
    {
        static async Task Main(string[] args)
        {

            var bus = Bus.Factory.CreateUsingRabbitMq(configurator =>
            {
                var host = configurator.Host("localhost", "/", x =>
                {
                    x.Password("guest");
                    x.Username("guest");
                });
            });

            await bus.StartAsync();

            for (int i = 0; i < 100; i++)
            {
                await bus.Publish<IMessage>(new { Message = $"sample message-{i}" }).ConfigureAwait(false);
            }

            await bus.StopAsync();
        }
    }
}


namespace MasstransitHost
{
    public interface IMessage
    {
        string Message { get; set; }
    }
}
