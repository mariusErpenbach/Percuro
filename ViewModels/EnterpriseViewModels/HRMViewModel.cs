using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Percuro.ViewModels.EnterpriseViewModels.HRM;

namespace Percuro.ViewModels.EnterpriseViewModels
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
        }        [RelayCommand]
        public void ToMitarbeiterverwaltungView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new MitarbeiterverwaltungViewModel();
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
