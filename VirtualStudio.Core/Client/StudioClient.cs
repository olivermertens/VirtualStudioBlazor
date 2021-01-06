using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Abstractions;
using VirtualStudio.Core.Client.Abstractions;

namespace VirtualStudio.Core.Client
{
    public class StudioClient : IStudioClient
    {
        private Dictionary<Type, IStudioClientMessageHandler> messageHandlers;
        private ClientStudioComponent component;
        private readonly IStudioClientConnection connection;

        public bool RegisterMessageHandler(IStudioClientMessageHandler messageHandler)
        {
            Type messageHandlerType = messageHandler.GetType().GetInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IStudioClientMessageHandler<,>));
            if (messageHandlerType != null)
            {
                Type messageType = messageHandlerType.GenericTypeArguments[0];
                messageHandlers[messageType] = messageHandler;
                return true;
            }
            else
                return false;
        }

        private Task<T> Connection_MessageReceived<T>(IStudioClientMessage<T> message)
        {
            var messageHandlerKvp = messageHandlers.FirstOrDefault(c => c.Key.IsAssignableFrom(message.GetType()));
            if (messageHandlerKvp.Value != null)
            {
                var messageHandler = messageHandlerKvp as IStudioClientMessageHandler<IStudioClientMessage<T>, T>;
                return messageHandler.HandleMessageAsync(message);
            }
            return Task.FromResult(default(T));
        }

        public Task<T> HandleMessageAsync<T>(IStudioClientMessage<T> message)
            => connection.SendMessageAsync(message);

        public ClientStudioComponent GetComponent() => component;
    }
}
