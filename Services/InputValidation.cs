namespace Percuro.Services;

public static class InputValidation
{
    public static bool IsValidString(string? input) =>
        !string.IsNullOrWhiteSpace(input) && System.Text.RegularExpressions.Regex.IsMatch(input, "^[a-zA-ZäöüÄÖÜß ]+$");

    public static bool IsValidNumber(string? input) =>
        int.TryParse(input, out _);

    public static bool IsValidPostalCode(string? input) =>
        !string.IsNullOrWhiteSpace(input) && input.Length <= 6 && int.TryParse(input, out _);

    public static bool IsValidEmail(string? email) =>
        !string.IsNullOrWhiteSpace(email) &&
        System.Text.RegularExpressions.Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");

    public static bool IsValidPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return false;

        // Remove all non-numeric characters
        phoneNumber = System.Text.RegularExpressions.Regex.Replace(phoneNumber, "[^0-9]", "");

        return !string.IsNullOrEmpty(phoneNumber);
    }

    public static bool IsValidName(string? name) =>
        !string.IsNullOrWhiteSpace(name) && name.Length > 1 && System.Text.RegularExpressions.Regex.IsMatch(name, "^[a-zA-ZäöüÄÖÜß]+$");
}