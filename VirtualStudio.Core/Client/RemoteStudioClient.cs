using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VirtualStudio.Core.Client.Abstractions;

namespace VirtualStudio.Core.Client
{
    public class RemoteStudioClient : IDisposable
    {
        private Dictionary<Type, IStudioClientMessageHandler> messageHandlers;
        private readonly IStudioClientConnection connection;

        public RemoteStudioClient(IStudioClientConnection connection)
        {
            this.connection = connection;
            this.connection.MessageReceivedHandler = Connection_MessageReceived;
        }

        public void Dispose()
        {
            connection.MessageReceivedHandler = null;
        }

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
            var commandHandlerKvp = messageHandlers.FirstOrDefault(c => c.Key.IsAssignableFrom(message.GetType()));
            if(commandHandlerKvp.Value != null)
            {
                var messageHandler = commandHandlerKvp as IStudioClientMessageHandler<IStudioClientMessage<T>, T>;
                return messageHandler.HandleMessageAsync(message);
            }
            return Task.FromResult(default(T));
        }
    }
}
