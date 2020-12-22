using System;
using System.Collections.ObjectModel;

namespace VirtualStudio.Server
{
    public class StudioEndpointCollection : Collection<StudioEndpoint>
    {
        public StudioComponent StudioComponent { get; }

        public StudioEndpointCollection(StudioComponent studioComponent)
        {
            StudioComponent = studioComponent;
        }

        public bool ContainsItemWithName(string name)
        {
            foreach (var item in this)
            {
                if (item.Name == name)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Exists(Predicate<StudioEndpoint> condition)
        {
            foreach (var item in this)
            {
                if (condition.Invoke(item))
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryGetLink(int linkId, out StudioLink studioLink)
        {
            foreach(var item in this)
            {
                foreach(var link in item.Links)
                {
                    if(link.Id == linkId)
                    {
                        studioLink = link;
                        return true;
                    }
                }
            }
            studioLink = null;
            return false;
        }

        protected override void ClearItems()
        {
            foreach (var item in this)
            {
                item.ContainingCollection = null;
            }
            base.ClearItems();
        }

        protected override void InsertItem(int index, StudioEndpoint item)
        {
            base.InsertItem(index, item);
            item.ContainingCollection = this;
        }

        protected override void RemoveItem(int index)
        {
            this[index].ContainingCollection = null;
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, StudioEndpoint item)
        {
            this[index].ContainingCollection = null;
            base.SetItem(index, item);
            item.ContainingCollection = this;
        }
    }
}