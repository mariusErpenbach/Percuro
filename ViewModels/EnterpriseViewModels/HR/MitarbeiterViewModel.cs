using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class MitarbeiterViewModel : ViewModelBase
{
    // Basic ViewModel for MitarbeiterView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRViewModel();
        }
    }
}