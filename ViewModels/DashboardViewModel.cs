using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels;
public partial class DashboardViewModel : ViewModelBase
{
  public string WelcomeMessage => $"Willkommen, {UserSession.Current.Username}, {UserSession.Current.Role}!";
    
    public DashboardViewModel()
    {
        
    }
     
     
}