using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels.EnterpriseViewModels.FRM
{
    public partial class RechnungenViewModel : ViewModelBase
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