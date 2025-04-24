using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.Finance
{
    public partial class BuchhaltungViewModel : ViewModelBase
    {
    [RelayCommand]
    public void ToFinanceView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new FinanceViewModel();
        }
    }
    }
}