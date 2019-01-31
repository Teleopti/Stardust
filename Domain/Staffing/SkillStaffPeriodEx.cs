using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class SkillStaffPeriodEx : SkillStaffPeriod
	{
		public double AnsweredWithinSeconds { get; set; }
		public double Forecast { get; set; }
		private double BookedAgainstIncomingDemand65 { get; set; }
		private double _calculatedResource;
		public Guid SkillId { get; set; }
		
		public SkillStaffPeriodEx(){}
		
		private SkillStaffPeriodEx(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData) : base(period, taskData, serviceAgreementData)
		{
		}

		public new void SetCalculatedResource65(double resources)
		{
			_calculatedResource = resources;
			
			if (SegmentInThisCollection.Count == 1)
			{
				PickResources65();
			}
			else
			{
				var sortedSegmentInThisCollection =
					SegmentInThisCollection.OrderBy(s => s.BelongsTo.Period.StartDateTime).ToArray();
				foreach (var ySegment in sortedSegmentInThisCollection)
				{
					ySegment.BelongsTo.PickResources65();
				}
			}
		}
		
		private static void addToLists(double trafficToDistribute, ISkillStaffPeriod currentPeriod, ISkillStaffPeriod ourPeriod)
		{
			ISkillStaffSegment newSegment = new SkillStaffSegment(trafficToDistribute);
			ISkillStaffSegmentPeriod newSegmentPeriod =
				new SkillStaffSegmentPeriod(ourPeriod, currentPeriod, newSegment, currentPeriod.Period);
			((SkillStaffPeriod) ourPeriod)._sortedSegmentCollection.Add(newSegmentPeriod.Period.StartDateTime, newSegmentPeriod);

			var skillStaffPeriod = (SkillStaffPeriod) currentPeriod;
			skillStaffPeriod._segmentInThisCollection.Add(newSegmentPeriod);
			skillStaffPeriod._bookedResource65 += newSegmentPeriod.BookedResource65;
		}

		private void createSkillStaffSegments65(IList<SkillStaffPeriodEx> sortedPeriods, int currentIndex)
		{
			var ourPeriod = sortedPeriods[currentIndex];
			var sa = TimeSpan.FromSeconds(AnsweredWithinSeconds);//Payload.ServiceAgreementData.ServiceLevel.Seconds);

			for (; currentIndex < sortedPeriods.Count; currentIndex++)
			{
				var interval = sortedPeriods[currentIndex];
				var currentLength = interval.DateTimePeriod.ElapsedTime();
				addToLists(0, interval, ourPeriod);

				sa = sa.Add(-currentLength);

				if (sa <= TimeSpan.Zero) break;
			}
		}

		public void CreateSegmentCollection(IList<SkillStaffPeriodEx> intervalList)
		{
			//intervalList.First().DateTimePeriod
			
			if (intervalList != null)
			{
				int thisIndex = intervalList.IndexOf(this);
				if (thisIndex == -1)
					throw new ArgumentException("List with skillstaffperiods must contain working skillstaffperiod");
				//RemoveExistingSegments(periods, thisIndex);
				createSkillStaffSegments65(intervalList, thisIndex);
			}
		}
		
		public new void PickResources65()
		{
			//_estimatedServiceLevel = null;
			//_estimatedServiceLevelShrinkage = null;

			//SkillStaff thisSkillStaff = (SkillStaff) Payload;

			if (SortedSegmentCollection.Count == 1)
			{
				BookedAgainstIncomingDemand65 = _calculatedResource;
				SortedSegmentCollection[0].BookedResource65 = _calculatedResource;
				_bookedResource65 = _segmentInThisCollection.Sum(x => x.BookedResource65);
			}
			else
			{
				var forecastIncomingDemand = Forecast;

				BookedAgainstIncomingDemand65 = 0;

				foreach (var xSegment in SortedSegmentCollection)
				{
					((SkillStaffPeriod) xSegment.BelongsToY)._bookedResource65 -= xSegment.BookedResource65;
					xSegment.BookedResource65 = 0;

					var diff = forecastIncomingDemand - BookedAgainstIncomingDemand65;
					if (diff > 0)
					{
						ISkillStaffPeriod ownerSkillStaffPeriod = xSegment.BelongsToY;
						ISkillStaff ownerSkillStaff = ownerSkillStaffPeriod.Payload;
						var ownerSortedSegmentCollection = ownerSkillStaffPeriod.SortedSegmentCollection;
						if (ownerSortedSegmentCollection.Count == 0) continue;

						double ownerNotBookedResource = ownerSkillStaff.CalculatedResource -
														(ownerSkillStaffPeriod.BookedResource65 - ownerSortedSegmentCollection[0].BookedResource65);
						if (ownerNotBookedResource <= 0) continue;

						if (diff >= ownerNotBookedResource)
							diff = ownerNotBookedResource;

						BookedAgainstIncomingDemand65 += diff;
						xSegment.BookedResource65 = diff;
						((SkillStaffPeriod) xSegment.BelongsToY)._bookedResource65 += xSegment.BookedResource65;
					}
				}
			}
		}
	}
}