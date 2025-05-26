using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.Views.EnterpriseViews;
using Percuro.ViewModels.EnterpriseViewModels.FRM;

namespace Percuro.ViewModels.EnterpriseViewModels
{
    public partial class FRMViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToBilanzView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new BilanzViewModel();
            }
        }

        [RelayCommand]
        public void ToBuchhaltungView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new BuchhaltungViewModel();
            }
        }

        [RelayCommand]
        public void ToBudgetPlanningView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new BudgetPlanningViewModel();
            }
        }

        [RelayCommand]
        public void ToRechnungenView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new RechnungenViewModel();
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