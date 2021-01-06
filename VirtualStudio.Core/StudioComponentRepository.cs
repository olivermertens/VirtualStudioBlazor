using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class StudioComponentRepository
    {
        int idCounter = 1;
        protected int GetNewId() => idCounter++;

        public event EventHandler<StudioComponent> ClientAdded;
        public event EventHandler<StudioComponent> ClientRemoved;
        public event EventHandler<StudioComponent> PlaceholderAdded;
        public event EventHandler<StudioComponent> PlaceholderRemoved;

        protected List<PlaceholderStudioComponent> _placeholders;
        public IReadOnlyCollection<PlaceholderStudioComponent> Placeholders { get; }
        protected List<StudioComponent> _clients;
        public IReadOnlyCollection<StudioComponent> Clients { get; }

        public StudioComponentRepository(List<PlaceholderStudioComponent> placeholders = null)
        {
            _placeholders = new List<PlaceholderStudioComponent>();
            if (placeholders != null)
            {
                foreach (var placeholder in placeholders)
                {
                    AddPlaceholder(placeholder);
                }
            }
            Placeholders = _placeholders.AsReadOnly();
            _clients = new List<StudioComponent>();
            Clients = _clients.AsReadOnly();
        }

        public virtual bool AddClient(StudioComponent client)
        {
            if (AddStudioComponent(client, true))
            {
                ClientAdded?.Invoke(this, client);
                return true;
            }
            return false;
        }

        public virtual void RemoveClient(StudioComponent client)
        {
            if (_clients.Remove(client))
            {
                ClientRemoved?.Invoke(this, client);
            }
        }

        public virtual bool AddPlaceholder(PlaceholderStudioComponent placeholder)
        {
            if(AddStudioComponent(placeholder, false))
            {
                PlaceholderAdded?.Invoke(this, placeholder);
                return true;
            }
            return false;
        }

        public virtual void RemovePlaceholder(PlaceholderStudioComponent placeholder)
        {
            if (_placeholders.Remove(placeholder))
            {
                PlaceholderRemoved?.Invoke(this, placeholder);
            }
        }

        public bool Contains(StudioComponent component)
        {
            return _clients.Contains(component) ||
                   (component is PlaceholderStudioComponent psc && _placeholders.Contains(psc));
        }

        public bool Exists(Predicate<StudioComponent> match)
        {
            return _clients.Exists(match) || _placeholders.Exists(match);
        }

        public StudioComponent Find(Predicate<StudioComponent> match)
        {
            var found = _clients.Find(match);
            if(found == null)
            {
                found = _placeholders.Find(match);
            }
            return found;
        }

        protected virtual bool AddStudioComponent(StudioComponent component, bool isClient)
        {
            if (Contains(component))
            {
                return false;
            }
            if (component.Id != 0)
            {
                if (Exists(c => c.Id == component.Id))
                {
                    throw new ArgumentException("Cannot add a client with an already existing ID.");
                }
            }
            else
            {
                do
                {
                    component.Id = GetNewId();
                } while (Exists(c => c.Id == component.Id));
            }
            if (isClient)
            {
                _clients.Add(component);
            }
            else
            {
                if (component is PlaceholderStudioComponent psc)
                {
                    _placeholders.Add(psc);
                }
                else
                {
                    throw new ArgumentException("Component cannot be added as placeholder.");
                }
            }
            return true;
        }
    }
}
