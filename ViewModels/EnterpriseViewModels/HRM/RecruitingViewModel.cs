using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM;

public partial class RecruitingViewModel : ViewModelBase
{
    // Basic ViewModel for RecruitingView
    
    [RelayCommand]
    public void ToHRMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRMViewModel();
        }
    }
}