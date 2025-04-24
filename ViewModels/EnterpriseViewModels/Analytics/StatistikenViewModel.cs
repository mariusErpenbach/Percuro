using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.Analytics
{
    public partial class StatistikenViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToAnalyticsView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new AnalyticsViewModel();
            }
        }
    }
}