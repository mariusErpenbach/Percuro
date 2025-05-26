using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.CRM;

public partial class CustomerManagementViewModel : ViewModelBase
{
    // Basic ViewModel for CustomerManagementView
    [RelayCommand]
        public void ToCRMView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new CRMViewModel();
            }
        }
}