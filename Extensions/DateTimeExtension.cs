using System;
namespace API.Extensions
{
    public static class DateTimeExtensions
    {
        // Keyword "this" allow to call the static method in any DateTime props
        public static int CalculateAge(this DateTime dob)
        {
            var today = DateTime.Today;
            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--;
            return age;
        }
    }
}
