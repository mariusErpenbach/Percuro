using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.CRM;

public partial class SalesManagementViewModel : ViewModelBase
{
    // ViewModel für CRM (ehemals SalesAndCRM)
    [RelayCommand]
    public void ToCRMMainView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new CRMViewModel();
        }
    }
}