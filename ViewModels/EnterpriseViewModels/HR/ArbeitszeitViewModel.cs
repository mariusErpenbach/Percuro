using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.HR;

public partial class ArbeitszeitViewModel : ViewModelBase
{
    // Basic ViewModel for ArbeitszeitView
    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRViewModel();
        }
    }
}