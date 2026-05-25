using System;
using System.Globalization;

namespace ConsoleApp1_Mandaue
{
    // =========================================================================
    //  COMPONENT: ValidationService
    //  Validates all user input before it is saved to a record.
    // =========================================================================
    class ValidationService
    {
        // Name must be between 2 and 100 characters
        public bool IsValidName(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            string trimmed = name.Trim();
            return trimmed.Length >= 2 && trimmed.Length <= 100;
        }

        // Date must be in yyyy-MM-dd format (e.g. 2026-06-15)
        public bool IsValidDate(string date)
        {
            if (string.IsNullOrWhiteSpace(date)) return false;
            return DateTime.TryParseExact(
                date.Trim(), "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }

        // Time must be in 24-hour HH:mm format — used to validate the stored value
        public bool IsValidTime(string time)
        {
            if (string.IsNullOrWhiteSpace(time)) return false;
            return DateTime.TryParseExact(
                time.Trim(), "HH:mm",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None, out _);
        }

        // Reason must be between 3 and 200 characters
        public bool IsValidReason(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason)) return false;
            string trimmed = reason.Trim();
            return trimmed.Length >= 3 && trimmed.Length <= 200;
        }

        // Status must be one of the three valid values
        public bool IsValidStatus(string status)
        {
            return status == "Scheduled" ||
                   status == "Completed" ||
                   status == "Cancelled";
        }
    }
}
