using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM;

public partial class GehaltsabrechnungViewModel : ViewModelBase
{
    // Basic ViewModel for GehaltsabrechnungView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRMViewModel();
        }
    }
}