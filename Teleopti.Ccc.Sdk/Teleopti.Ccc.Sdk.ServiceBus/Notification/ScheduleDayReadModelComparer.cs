using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
    public interface IScheduleDayReadModelComparer
    {
        string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo);
    }

    public class ScheduleDayReadModelComparer : IScheduleDayReadModelComparer
    {
        public string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo)
        {
            string message = null;
            var weekDayName = cultureInfo.DateTimeFormat.DayNames[(int)newReadModel.StartDateTime.DayOfWeek];
            var startDateTime = string.Format(cultureInfo, newReadModel.StartDateTime.ToString(cultureInfo));
            var startDate = string.Format(cultureInfo, newReadModel.StartDateTime.Date.ToString(cultureInfo));
            var endDateTime = string.Format(cultureInfo, newReadModel.EndDateTime.ToString(cultureInfo));
            
            
            // Check if the working day is change to Off day.
            if (newReadModel.Workday != existingReadModel.Workday)
            {
                // if chnage working day to an Off day.
                if (!newReadModel.Workday)
                {
                    message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + startDate + string.Format(cultureInfo, UserTexts.Resources.NotWorking);
                }
                else  // From Off Day to Working day.
                {
                    message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + startDateTime + "-" + endDateTime;
                }

                return message;
            }
            
            // If new and existing both are WORKING Days but the shift start or end time is changed.
            if ((newReadModel.StartDateTime != existingReadModel.StartDateTime) || (newReadModel.EndDateTime != existingReadModel.EndDateTime))
            {
                message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + startDateTime + "-" + endDateTime;
            }

            return message;
        }
    }
}
