using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace Percuro.ViewModels;

public partial class LoginViewModel : ViewModelBase{
    
    [ObservableProperty]
    private string _username ="";

    [ObservableProperty]
    private string _password = "";

    [ObservableProperty]
    private string _selectedRole = "Role A";
    [ObservableProperty]
    private List<string> _availableRoles = new() {"Role A","Role B"};

    [RelayCommand]
    private void Login(){
        Console.WriteLine("yo");
    }


}

