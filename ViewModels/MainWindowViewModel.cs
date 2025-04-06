using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Percuro.ViewModels;
public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase? _currentViewModel;
    
    public MainWindowViewModel()
    {
        // WelcomeView as starting View
        CurrentViewModel = new WelcomeViewModel{Parent = this};
    }
}