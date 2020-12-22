using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using VirtualStudio.Shared;

namespace VirtualStudio.Server
{
    public class StudioManager
    {
        public IReadOnlyList<StudioComponent> Components { get; }
        private List<StudioComponent> components;

        public IReadOnlyList<StudioLink> Links { get; }
        private List<StudioLink> links;
        Dictionary<StudioClient, StudioComponent> ClientComponentMapping;
        Dictionary<string, StudioClient> StudioClients;
        VirtualStudioHub virtualStudioHub;

        public StudioManager()
        {
            StudioClients = new Dictionary<string, StudioClient>();
            ClientComponentMapping = new Dictionary<StudioClient, StudioComponent>();

            components = new List<StudioComponent>();
            Components = components.AsReadOnly();

            links = new List<StudioLink>();
            Links = links.AsReadOnly();
        }


        public void RegisterHub(VirtualStudioHub virtualStudioHub)
        {
            this.virtualStudioHub = virtualStudioHub;
        }

        public StudioClient GetClient(string connectionId)
        {
            if (StudioClients.TryGetValue(connectionId, out StudioClient client))
            {
                return client;
            }
            return null;
        }

        public void AddStudioClient(StudioClient studioClient)
        {
            StudioClients[studioClient.ConnectionId] = studioClient;
        }

        public async Task RemoveStudioClient(StudioClient studioClient)
        {
            if(StudioClients.Remove(studioClient.ConnectionId))
            {
                var component = components.Find(c => c.StudioClient == studioClient);
                await component?.RemoveStudioClient();
            }
        }

        public void AddComponent(StudioComponent component)
        {
            components.Add(component);
        }

        public void RemoveComponent(StudioComponent component)
        {
            components.Remove(component);
        }

        public bool TryConnectEndpoints(StudioEndpoint output, StudioEndpoint input)
        {
            var fromComponent = output.ContainingCollection.StudioComponent;
            var toComponent = input.ContainingCollection.StudioComponent;
            if (!fromComponent.Outputs.Contains(output) ||
                !toComponent.Inputs.Contains(input))
            {
                return false;
            }

            var link = new StudioLink(output, input);

            links.Add(link);
            return true;
        }

        public void RemoveLink(StudioLink link)
        {
            // TODO: Disconnect real connection.

            link.RemoveEndpoints();
            links.Remove(link);
        }
    }
}