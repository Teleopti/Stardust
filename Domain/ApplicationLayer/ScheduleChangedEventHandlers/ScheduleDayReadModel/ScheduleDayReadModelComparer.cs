using System;
using System.Globalization;
using log4net;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleDayReadModel
{
    public interface IScheduleDayReadModelComparer
    {
        string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate);
    }

    public class ScheduleDayReadModelComparer : IScheduleDayReadModelComparer
    {
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ScheduleDayReadModelComparer));

		public string FindSignificantChanges(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel, CultureInfo cultureInfo, DateOnly currentDate)
		{
			if (Logger.IsInfoEnabled)
			{
				Logger.Info("newReadModel:" + (newReadModel == null ? "null" : newReadModel.ToJson()));
				Logger.Info("existingReadModel:" + (existingReadModel == null ? "null" : existingReadModel.ToJson()));
			}

			string weekDayName = cultureInfo.DateTimeFormat.GetDayName(currentDate.DayOfWeek);

			string currentDateFormat = currentDate.ToShortDateString(cultureInfo);

            //if new and existing both are OFF days
		    if ((newReadModel == null && existingReadModel == null) ||
		        (newReadModel == null && existingReadModel.Workday == false) ||
		        (existingReadModel == null && newReadModel.Workday == false))
		        return null;
		    if ((newReadModel != null && newReadModel.Workday == false) &&
		        (existingReadModel != null && existingReadModel.Workday == false)
				&& !isOvertimeAddedOnDayOff(newReadModel, existingReadModel))
		        return null;
            
                //if new ReadModel is NULL and existing Read Model is not NULL  (From Working Day to an OFF Day)
            if(newReadModel==null)
            {
				return $"{weekDayName} {currentDateFormat}{UserTexts.Resources.ResourceManager.GetString("NotWorking", cultureInfo)}";
            }

			string startDateTime = newReadModel.StartDateTime.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo);
			string endDateTime = newReadModel.EndDateTime.ToString(cultureInfo.DateTimeFormat.ShortTimePattern, cultureInfo);
               
            // if existingReadModel is NULL and new read model is NOT NULL  (From OFF Day to a working day)
            if (existingReadModel==null)
            {
                return $"{weekDayName} {currentDateFormat} {startDateTime}-{endDateTime}";
            }

            if(newReadModel.Workday!= existingReadModel.Workday)
            {
                // if change working day to an Off day.
                if (!newReadModel.Workday)
                {
                    return $"{weekDayName} {currentDateFormat} {UserTexts.Resources.ResourceManager.GetString("NotWorking", cultureInfo)}";
                }
                else  // From Off Day to Working day.
                {
                    return $"{weekDayName} {currentDateFormat} {startDateTime}-{endDateTime}";
                }
            }

            // If new and existing both are WORKING Days but the shift start or end time is changed.
            if ((newReadModel.StartDateTime.Truncate(TimeSpan.FromMinutes(1)) != existingReadModel.StartDateTime.Truncate(TimeSpan.FromMinutes(1))) || (newReadModel.EndDateTime.Truncate(TimeSpan.FromMinutes(1)) != existingReadModel.EndDateTime.Truncate(TimeSpan.FromMinutes(1))))
            {
                return $"{weekDayName} {currentDateFormat} {startDateTime}-{endDateTime}";
            }

            return null;
        }

		private static bool isOvertimeAddedOnDayOff(ScheduleDayReadModel newReadModel, ScheduleDayReadModel existingReadModel)
		{
			var defaultValue = new ScheduleDayReadModel();
			var isDayOff = isNotWorkingDay(existingReadModel)
						   && defaultValue.StartDateTime.Equals(existingReadModel.StartDateTime)
						   && defaultValue.EndDateTime.Equals(existingReadModel.EndDateTime);
			if (!isDayOff)
				return false;

			var isOvertimeAdded = isNotWorkingDay(newReadModel)
								  && (!newReadModel.StartDateTime.Equals(defaultValue.StartDateTime)
									  || !newReadModel.EndDateTime.Equals(defaultValue.EndDateTime));
			return isOvertimeAdded;
		}

		private static bool isNotWorkingDay(ScheduleDayReadModel scheduleDayReadModel)
		{
			return !scheduleDayReadModel.Workday && scheduleDayReadModel.ContractTimeTicks.Equals(0L);
		}
	}
}
