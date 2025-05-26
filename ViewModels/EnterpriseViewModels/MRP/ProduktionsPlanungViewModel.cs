using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.MRP
{
    public partial class ProduktionsPlanungViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToMRPView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new MRPViewModel();
            }
        }
    }
}