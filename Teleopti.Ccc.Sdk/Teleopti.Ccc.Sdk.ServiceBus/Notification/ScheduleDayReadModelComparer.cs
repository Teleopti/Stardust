using System.Globalization;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Notification
{
    public interface IScheduleDayReadModelComparer
    {
        string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate);
    }

    public class ScheduleDayReadModelComparer : IScheduleDayReadModelComparer
    {
        public string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate)
        {
            string message = null;
            string weekDayName;
            string startDateTime;
            string endDateTime;

            //if new and existing both are OFF days
            if (newReadModel == null && existingReadModel == null)
                return null;
            
          
            //if new ReadModel is NULL and existing Read Model is not NULL  (From Working Day to an OFF Day)
            if(newReadModel==null)
            {
                weekDayName = cultureInfo.DateTimeFormat.DayNames[(int)currentDate.DayOfWeek];
                message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + currentDate + string.Format(cultureInfo, UserTexts.Resources.NotWorking);
                return message;
            }
               
            // if existingReadModel is NULL and new read model is NOT NULL  (From OFF Day to a working day)
            if (existingReadModel == null)
            {
                weekDayName = cultureInfo.DateTimeFormat.DayNames[(int)newReadModel.StartDateTime.DayOfWeek];
                startDateTime = string.Format(cultureInfo, newReadModel.StartDateTime.ToString(cultureInfo));
                endDateTime = string.Format(cultureInfo, newReadModel.EndDateTime.ToString(cultureInfo));

                message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + startDateTime + "-" + endDateTime;
                return message;
            }

            weekDayName = cultureInfo.DateTimeFormat.DayNames[(int)newReadModel.StartDateTime.DayOfWeek];
            startDateTime = string.Format(cultureInfo, newReadModel.StartDateTime.ToString(cultureInfo));
            endDateTime = string.Format(cultureInfo, newReadModel.EndDateTime.ToString(cultureInfo));
            
            // If new and existing both are WORKING Days but the shift start or end time is changed.
            if ((newReadModel.StartDateTime != existingReadModel.StartDateTime) || (newReadModel.EndDateTime != existingReadModel.EndDateTime))
            {
                message = string.Format(cultureInfo, UserTexts.Resources.YourWorkingHoursHaveChanged) + " " + weekDayName + " " + startDateTime + "-" + endDateTime;
            }

            return message;
        }
    }
}
