using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using VirtualStudio.Shared.DTOs;

namespace VirtualStudio.ViewModels
{
    public class StudioComponentRepositoryViewModel
    {
        public ObservableCollection<PlaceholderViewModel> Placeholders { get; } = new ObservableCollection<PlaceholderViewModel>();
        public ObservableCollection<ComponentViewModel> Clients { get; } = new ObservableCollection<ComponentViewModel>();

        public StudioComponentRepositoryViewModel(StudioComponentRepositoryDto studioComponentRepositoryDto)
        {
            foreach (var client in studioComponentRepositoryDto.Clients)
                Clients.Add(new ComponentViewModel(client));
            foreach (var placeholder in studioComponentRepositoryDto.Placeholders)
                Placeholders.Add(new PlaceholderViewModel(placeholder));
        }
    }
}
