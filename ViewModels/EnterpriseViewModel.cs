using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Threading;
using Percuro.ViewModels.EnterpriseViewModels;
using Percuro.ViewModels.EnterpriseViewModels.HRM;

namespace Percuro.ViewModels;
public partial class EnterpriseViewModel : ViewModelBase
{
    public EnterpriseViewModel()
    {
    
    }

    [RelayCommand]
    public void ToFRMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new FRMViewModel();
        }
    }

    [RelayCommand]
    public void ToHRView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new HRMViewModel();
        }
    }

    [RelayCommand]
    public void ToProductionView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MRPViewModel();
        }
    }

    [RelayCommand]
    public void ToSalesAndCRMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new CRMViewModel();
        }
    }

    [RelayCommand]
    public void ToAnalyticsView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new AnalyticsViewModel();
        }
    }
        [RelayCommand]
    
    public void ToDashboardView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new DashboardViewModel();
        }
    }

    [RelayCommand]
    public void ToMRPView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new MRPViewModel();
        }
    }

    [RelayCommand]
    public void ToSCMView()
    {
        if (Parent is MainWindowViewModel mainVm)
        {
            mainVm.CurrentViewModel = new SCMViewModel();
        }
    }
}