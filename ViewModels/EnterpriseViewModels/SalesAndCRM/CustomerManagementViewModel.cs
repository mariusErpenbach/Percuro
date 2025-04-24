using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.SalesAndCRM;

public partial class CustomerManagementViewModel : ViewModelBase
{
    // Basic ViewModel for CustomerManagementView
    [RelayCommand]
        public void ToSalesAndCRMView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new SalesAndCRMViewModel();
            }
        }
}