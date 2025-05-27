using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Percuro.ViewModels.EnterpriseViewModels.HR
{
    public partial class CalendarDayViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _date;

        [ObservableProperty]
        private string? _dayNumber;

        [ObservableProperty]
        private bool _hasEntry;

        [ObservableProperty]
        private bool _isCurrentMonth; // Is this day part of the currently displayed month?

        [ObservableProperty]
        private bool _isToday;

        [ObservableProperty] // Added for hover effect
        private bool _isHovered;

        [ObservableProperty] // Added for selection state
        private bool _isSelected;

        public CalendarDayViewModel(DateTime date, bool isCurrentMonth, bool hasEntry)
        {
            Date = date;
            DayNumber = date.Day.ToString();
            IsCurrentMonth = isCurrentMonth;
            HasEntry = hasEntry;
            IsToday = date.Date == DateTime.Today;
        }
    }
}
