using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;

public class DateService
{
    public string GetCurrentMonth()
    {
        var culture = new CultureInfo("he-IL");
        return DateTime.Now.ToString("MMMM", culture);
    }
}