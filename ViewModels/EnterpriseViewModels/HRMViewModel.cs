using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.HRM
{
    public partial class HRMViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToEnterpriseView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new EnterpriseViewModel();
            }
        }

        [RelayCommand]
        public void ToArbeitszeitView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new ArbeitszeitViewModel();
            }
        }

        [RelayCommand]
        public void ToGehaltsabrechnungView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new GehaltsabrechnungViewModel();
            }
        }

        [RelayCommand]
        public void ToMitarbeiterView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new MitarbeiterViewModel();
            }
        }

        [RelayCommand]
        public void ToRecruitingView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new RecruitingViewModel();
            }
        }
    }
}
