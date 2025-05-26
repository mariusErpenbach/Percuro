using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.MRP
{
    public partial class EinkaufViewModel : ViewModelBase
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