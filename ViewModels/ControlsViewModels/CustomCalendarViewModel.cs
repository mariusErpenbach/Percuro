using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Percuro.ViewModels.EnterpriseViewModels.HR; // Assuming CalendarDayViewModel is here

namespace Percuro.ViewModels.ControlsViewModels
{
    public enum CalendarSelectionMode { Day, Week } // Added enum

    public partial class CustomCalendarViewModel : ObservableObject
    {
        [ObservableProperty]
        private DateTime _currentDisplayMonth;

        [ObservableProperty]
        private ObservableCollection<string> _dayNames = new();

        [ObservableProperty]
        private ObservableCollection<CalendarDayViewModel> _calendarDays = new(); // Reusing CalendarDayViewModel

        [ObservableProperty]
        private string _currentMonthYearDisplay = string.Empty;

        [ObservableProperty] // Added this property
        private CalendarSelectionMode _currentSelectionMode = CalendarSelectionMode.Day;

        public CustomCalendarViewModel()
        {
            CurrentDisplayMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            // Ensure DayNames are initialized correctly for the current culture, starting with Monday
            var dayNames = CultureInfo.CurrentCulture.DateTimeFormat.AbbreviatedDayNames.ToList();
            // Adjust to start from Monday if Sunday is the first day
            if (CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Sunday)
            {
                var sunday = dayNames[0];
                dayNames.RemoveAt(0);
                dayNames.Add(sunday);
            }
            DayNames = new ObservableCollection<string>(dayNames);
            LoadCalendarDays();
        }

        private void LoadCalendarDays()
        {
            CalendarDays.Clear();
            CurrentMonthYearDisplay = CurrentDisplayMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture);

            var firstDayOfMonth = new DateTime(CurrentDisplayMonth.Year, CurrentDisplayMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(CurrentDisplayMonth.Year, CurrentDisplayMonth.Month);
            // Calculate the correct starting day of the week, ensuring Monday is the first column
            int dayOfWeek = (int)firstDayOfMonth.DayOfWeek;
            if (CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek == DayOfWeek.Sunday)
            {
                dayOfWeek = (dayOfWeek == 0) ? 6 : dayOfWeek - 1; // Convert Sunday (0) to 6, others shift down
            }
            else
            {
                 dayOfWeek = (dayOfWeek == (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) ? 0 : (dayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7;
            }


            // Add days from previous month
            for (int i = 0; i < dayOfWeek; i++)
            {
                var prevMonthDay = firstDayOfMonth.AddDays(-(dayOfWeek - i));
                CalendarDays.Add(new CalendarDayViewModel(prevMonthDay, false, false)); // isCurrentMonth = false, hasEntry = false
            }

            // Add days of current month
            for (int i = 1; i <= daysInMonth; i++)
            {
                var date = new DateTime(CurrentDisplayMonth.Year, CurrentDisplayMonth.Month, i);
                CalendarDays.Add(new CalendarDayViewModel(date, true, false)); // isCurrentMonth = true, hasEntry = false
            }

            // Add days from next month to fill the 6x7 grid
            int remainingCells = 42 - CalendarDays.Count; // 7 cols * 6 rows = 42
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Correctly get the last day of the current month
            for (int i = 1; i <= remainingCells; i++)
            {
                CalendarDays.Add(new CalendarDayViewModel(lastDayOfMonth.AddDays(i), false, false)); // isCurrentMonth = false, hasEntry = false
            }
        }

        [RelayCommand]
        private void PreviousMonth()
        {
            CurrentDisplayMonth = CurrentDisplayMonth.AddMonths(-1);
            LoadCalendarDays();
        }

        [RelayCommand]
        private void NextMonth()
        {
            CurrentDisplayMonth = CurrentDisplayMonth.AddMonths(1);
            LoadCalendarDays();
        }
    }
}
