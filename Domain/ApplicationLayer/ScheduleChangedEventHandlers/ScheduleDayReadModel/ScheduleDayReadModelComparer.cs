﻿using System;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
    public interface IScheduleDayReadModelComparer
    {
        string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate);
    }

    public class ScheduleDayReadModelComparer : IScheduleDayReadModelComparer
    {
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate)
        {
            string message = null;
            string weekDayName = cultureInfo.DateTimeFormat.GetDayName(currentDate.DayOfWeek);;

			string currentDateFormat = currentDate.ToShortDateString(cultureInfo);

            //if new and existing both are OFF days
		    if ((newReadModel == null && existingReadModel == null) ||
		        (newReadModel == null && existingReadModel.Workday == false) ||
		        (existingReadModel == null && newReadModel.Workday == false))
		        return null;
		    if ((newReadModel != null && newReadModel.Workday == false) &&
		        (existingReadModel != null && existingReadModel.Workday == false))
		        return null;
            
                //if new ReadModel is NULL and existing Read Model is not NULL  (From Working Day to an OFF Day)
            if(newReadModel==null)
            {
				message = weekDayName + " " + currentDateFormat + UserTexts.Resources.ResourceManager.GetString("NotWorking", cultureInfo);
                return message;
            }

			string startDateTime = newReadModel.StartDateTime.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo);
			string endDateTime = newReadModel.EndDateTime.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo);
               
            // if existingReadModel is NULL and new read model is NOT NULL  (From OFF Day to a working day)
            if (existingReadModel==null)
            {
                message = weekDayName + " " + currentDateFormat + " " + startDateTime + "-" + endDateTime;
                return message;
            }

            if(newReadModel.Workday!= existingReadModel.Workday)
            {
                // if chnage working day to an Off day.
                if (!newReadModel.Workday)
                {
                    message = weekDayName + " " + currentDateFormat + " " + UserTexts.Resources.ResourceManager.GetString("NotWorking",cultureInfo);
                }
                else  // From Off Day to Working day.
                {
                    message = weekDayName + " " + currentDateFormat + " " + startDateTime + "-" + endDateTime;
                }
                return message;
            }

            // If new and existing both are WORKING Days but the shift start or end time is changed.
            if ((newReadModel.StartDateTime.Truncate(TimeSpan.FromMinutes(1)) != existingReadModel.StartDateTime.Truncate(TimeSpan.FromMinutes(1))) || (newReadModel.EndDateTime.Truncate(TimeSpan.FromMinutes(1)) != existingReadModel.EndDateTime.Truncate(TimeSpan.FromMinutes(1))))
            {
                message = weekDayName + " " + currentDateFormat + " " + startDateTime + "-" + endDateTime;
            }

            return message;
        }
    }
}
