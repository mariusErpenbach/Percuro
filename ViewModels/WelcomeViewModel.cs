// ViewModels/WelcomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
namespace Percuro.ViewModels;

public partial class WelcomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _welcomeMessage = "Herzlich willkommen <br/>";
     [ObservableProperty]
    private string _appName = "Percuro - ERP/CMS System";
    
    [RelayCommand]
    private void toLogin()
    {
        if (Parent is MainWindowViewModel mainVm){
            mainVm.CurrentViewModel = new LoginViewModel();
        }
    }
    public MainWindowViewModel?Parent{get;set;}
}