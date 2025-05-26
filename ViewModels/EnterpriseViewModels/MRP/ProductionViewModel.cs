using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Services;
using Percuro.ViewModels.EnterpriseViewModels.MRP;

namespace Percuro.ViewModels.EnterpriseViewModels
{
    public partial class MRPViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToEinkaufView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new EinkaufViewModel();
            }
        }

        [RelayCommand]
        public void ToInventoryView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new InventoryViewModel();
            }
        }

        [RelayCommand]
        public void ToProduktionsPlanungView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ProduktionsPlanungViewModel();
            }
        }

        [RelayCommand]
        public void ToEnterpriseView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new EnterpriseViewModel();
            }
        }
    }
}