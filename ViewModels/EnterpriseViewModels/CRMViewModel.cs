using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Controls;
using System;
using CommunityToolkit.Mvvm.Input;
using Percuro.ViewModels.EnterpriseViewModels.CRM;

namespace Percuro.ViewModels.EnterpriseViewModels
{
    public partial class CRMViewModel : ViewModelBase
    {
        [RelayCommand]
        public void ToCustomerManagementView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new CustomerManagementViewModel();
            }
        }

        [RelayCommand]
        public void ToSalesManagementView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new SalesManagementViewModel();
            }
        }

        [RelayCommand]
        public void ToEnterpriseView()
        {
            if (Parent is MainWindowViewModel mainVm)
            {
                mainVm.CurrentViewModel = new EnterpriseViewModel();
            }
        }
    }
}