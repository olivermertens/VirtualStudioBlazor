using System;
using System.Collections.Generic;
using System.Text;
using VirtualStudio.Core.Abstractions;

namespace VirtualStudio.Core
{
    public class StudioComponentRepository
    {
        IdGenerator idGenerator = new IdGenerator();

        public event EventHandler<IStudioComponent> ClientAdded;
        public event EventHandler<IStudioComponent> ClientRemoved;
        public event EventHandler<PlaceholderStudioComponent> PlaceholderAdded;
        public event EventHandler<PlaceholderStudioComponent> PlaceholderRemoved;

        protected List<PlaceholderStudioComponent> _placeholders;
        public IReadOnlyCollection<PlaceholderStudioComponent> Placeholders { get; }
        protected List<IStudioComponent> _clients;
        public IReadOnlyCollection<IStudioComponent> Clients { get; }

        public StudioComponentRepository()
        {
            _placeholders = new List<PlaceholderStudioComponent>();
            Placeholders = _placeholders.AsReadOnly();
            _clients = new List<IStudioComponent>();
            Clients = _clients.AsReadOnly();
        }

        public bool AddPlaceholder(PlaceholderStudioComponent placeholder)
        {
            if(AddPlaceholderStudioComponent(placeholder))
            {
                PlaceholderAdded?.Invoke(this, placeholder);
                return true;
            }
            return false;
        }

        public virtual bool AddClient(IStudioComponent client)
        {
            if (AddClientStudioComponent(client))
            {
                ClientAdded?.Invoke(this, client);
                return true;
            }
            return false;
        }

        public virtual void RemoveClient(IStudioComponent client)
        {
            if (_clients.Remove(client))
            {
                ClientRemoved?.Invoke(this, client);
            }
        }

        public virtual void RemovePlaceholder(PlaceholderStudioComponent placeholder)
        {
            if (_placeholders.Remove(placeholder))
            {
                PlaceholderRemoved?.Invoke(this, placeholder);
            }
        }

        public bool Contains(IStudioComponent component)
        {
            return _clients.Contains(component) || (component is PlaceholderStudioComponent psc && _placeholders.Contains(psc));
        }

        public bool Exists(Predicate<IStudioComponent> match)
        {
            return _clients.Exists(match) || _placeholders.Exists(match);
        }

        public IStudioComponent Find(Predicate<IStudioComponent> match)
        {
            var found = _clients.Find(match);
            if (found == null)
            {
                found = _placeholders.Find(match);
            }
            return found;
        }

        internal PlaceholderStudioComponent GetPlaceholderClone(PlaceholderStudioComponent placeholderComponent)
        {
            var clone = placeholderComponent.Clone();
            clone.SetId(idGenerator.GetNewId());
            return clone;
        }

        private bool PrepareAddComponent(IStudioComponent component)
        {
            if (Contains(component))
            {
                return false;
            }
            if (component.Id != 0)
            {
                if (Exists(c => c.Id == component.Id))
                {
                    throw new ArgumentException("Cannot add a placeholder component with an already existing ID.");
                }
            }
            else
            {
                do
                {
                    component.SetId(idGenerator.GetNewId());
                } while (Exists(c => c.Id == component.Id));
            }
            return true;
        }

        protected virtual bool AddPlaceholderStudioComponent(PlaceholderStudioComponent component)
        {
            if (!PrepareAddComponent(component))
                return false;

            _placeholders.Add(component);
            return true;
        }

        protected virtual bool AddClientStudioComponent(IStudioComponent component)
        {
            if (!PrepareAddComponent(component))
                return false;

            _clients.Add(component);
            return true;
        }
    }
}
