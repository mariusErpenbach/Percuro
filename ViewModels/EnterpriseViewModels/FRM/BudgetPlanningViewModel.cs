using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.FRM
{
    public partial class BudgetPlanningViewModel : ViewModelBase
    {
    [RelayCommand]
    public void ToFRMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new FRMViewModel();
        }
    }
    }
}