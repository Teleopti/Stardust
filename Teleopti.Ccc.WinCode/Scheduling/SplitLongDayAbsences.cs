﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public interface ISplitLongDayAbsences
	{
		IList<IPersonAbsence> SplitAbsences(IScheduleDay dayToSplit);
	}

	public class SplitLongDayAbsences : ISplitLongDayAbsences
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IList<IPersonAbsence> SplitAbsences(IScheduleDay dayToSplit)
		{
			IList<IPersonAbsence> dayAbsences = new List<IPersonAbsence>();

			foreach (IPersonAbsence personAbsence in dayToSplit.PersonAbsenceCollection())
			{
				IAbsenceLayer personAbsenceLayer = personAbsence.Layer;
				DateTimePeriod personAbsencePeriod = personAbsenceLayer.Period;
				DateTime personAbsencePeriodStart = personAbsencePeriod.StartDateTime;
				DateTime personAbsencePeriodEnd = personAbsencePeriod.EndDateTime;
				DateTimePeriod dayToSplityPeriod = dayToSplit.Period;
				DateTime dayToSplitPeriodStart = dayToSplityPeriod.StartDateTime;
				DateTime dayToSplitPeriodEnd = dayToSplityPeriod.EndDateTime;

				if (personAbsencePeriodStart < dayToSplitPeriodStart || personAbsencePeriodEnd > dayToSplitPeriodEnd)
				{
					IPersonAbsence splitDayAbsence = personAbsence.NoneEntityClone();
					DateTime start;
					DateTime end;
					if (personAbsencePeriodStart < dayToSplitPeriodStart)
						start = dayToSplitPeriodStart;
					else
						start = personAbsencePeriodStart;

					if (personAbsencePeriodEnd > dayToSplitPeriodEnd)
						end = dayToSplitPeriodEnd;
					else
						end = personAbsencePeriodEnd;

					if (end > start)
					{
						splitDayAbsence.Layer.Period = new DateTimePeriod(start, end);
						dayAbsences.Add(splitDayAbsence);
					}
				}
			}
			return dayAbsences;
		}
	}
}