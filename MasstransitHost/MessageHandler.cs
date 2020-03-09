using MassTransit;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace MasstransitHost
{
    public class MessageHandler : IConsumer<IMessage>
    {
        private ILogger<MessageHandler> _logger;
        public MessageHandler(ILogger<MessageHandler> logger)
        {
            _logger = logger;
        }
        public async Task Consume(ConsumeContext<IMessage> context)
        {
            _logger.LogInformation(context.Message.Message);
            await context.ConsumeCompleted;
        }
    }
}
