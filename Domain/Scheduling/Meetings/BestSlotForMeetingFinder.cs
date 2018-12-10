using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Meetings
{
	public class BestMeetingSlotResult
	{
		public double SlotValue { get; set; }
		public DateTimePeriod SlotPeriod { get; set; }
		public TimeSpan SlotLength { get; set; }
	}

	public interface IBestSlotForMeetingFinder
	{
		IList<BestMeetingSlotResult> FindBestSlot(IList<IPerson> requiredPersons, DateTimePeriod searchPeriod, TimeSpan meetingLength, int stepLength);
	}

	public class BestSlotForMeetingFinder : IBestSlotForMeetingFinder
	{
		private readonly IMeetingSlotImpactCalculator _meetingSlotImpactCalculator;

		public BestSlotForMeetingFinder(IMeetingSlotImpactCalculator meetingSlotImpactCalculator)
		{
			_meetingSlotImpactCalculator = meetingSlotImpactCalculator;
		}

		public IList<BestMeetingSlotResult> FindBestSlot(IList<IPerson> requiredPersons, DateTimePeriod searchPeriod, TimeSpan meetingLength, int stepLength)
		{
			var resultList = new List<BestMeetingSlotResult>();
			var meetingPeriod = new DateTimePeriod(searchPeriod.StartDateTime, searchPeriod.StartDateTime.Add(meetingLength));

			while (meetingPeriod.EndDateTime <= searchPeriod.EndDateTime)
			{
				var impact = _meetingSlotImpactCalculator.GetImpact(requiredPersons, meetingPeriod);
				if (impact.HasValue)
					resultList.Add(new BestMeetingSlotResult { SlotPeriod = meetingPeriod, SlotValue = impact.Value, SlotLength = meetingLength });
				meetingPeriod = meetingPeriod.MovePeriod(TimeSpan.FromMinutes(stepLength));
			}

			resultList.Sort((p1, p2) => p2.SlotValue.CompareTo(p1.SlotValue));
			return resultList;
		}
	}
}