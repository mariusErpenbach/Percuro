using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.Finance
{
    public partial class RechnungenViewModel : ViewModelBase
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