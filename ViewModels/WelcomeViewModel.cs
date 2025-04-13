// ViewModels/WelcomeViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using Percuro.Services;

namespace Percuro.ViewModels;

public partial class WelcomeViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _welcomeMessage = "Herzlich willkommen";
     [ObservableProperty]
    private string _appName = "Percuro - ERP/CMS System";
    
    [RelayCommand]
    private void toLogin()
    {
        if (Parent is MainWindowViewModel mainVm){
            var databaseService = new DatabaseService();
            mainVm.CurrentViewModel = new LoginViewModel(databaseService);
        }
    }
    public MainWindowViewModel?Parent{get;set;}
}