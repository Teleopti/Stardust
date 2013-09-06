﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
	public interface IHourlyAvailabilityPersonFilter
	{
		IList<IPerson> GetFilterdPerson(IList<IScheduleDay> scheduleDaysList, TimeSpan filterStartTime, TimeSpan filterEndTime);
	}

	public class HourlyAvailabilityPersonFilter : IHourlyAvailabilityPersonFilter
	{
		public IList<IPerson> GetFilterdPerson(IList<IScheduleDay> scheduleDaysList, TimeSpan filterStartTime, TimeSpan filterEndTime)
		{
			var personList = new List<IPerson>();
			var overnightFilter = isPeriodOvernight(filterEndTime);
			foreach (var scheduleDay in scheduleDaysList)
			{
				var studentAvailabilityDay =
					scheduleDay.PersistableScheduleDataCollection().OfType<IStudentAvailabilityDay>().FirstOrDefault();
				if (studentAvailabilityDay != null)
				{
					var restriction = studentAvailabilityDay.RestrictionCollection.FirstOrDefault();

					if (restriction != null)
					{
						var startTime = restriction.StartTimeLimitation.StartTime.GetValueOrDefault();
						//var startTime = overtimeAvailability.StartTime.GetValueOrDefault();

						var endTime = restriction.EndTimeLimitation.EndTime.GetValueOrDefault();
						//var endTime = overtimeAvailability.EndTime.GetValueOrDefault();
						var overnightShift = isPeriodOvernight(endTime);

						if ((overnightShift && overnightFilter) || (!overnightShift && !overnightFilter))
						{
							if (startTime <= filterStartTime && endTime >= filterEndTime)
								personList.Add(studentAvailabilityDay.Person);
						}
					}

				}
			}
			return personList;
		}

		private bool isPeriodOvernight(TimeSpan filterEndTime)
		{
			return filterEndTime.Days > 0;
		}
	}
}
