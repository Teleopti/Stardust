using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class SkillStaffPeriodEx : SkillStaffPeriod
	{
		public double AnsweredWithinSeconds { get; set; }
		public Guid SkillId { get; set; }

		public double Forecast
		{
			get
			{
				SkillStaff skillStaff = (SkillStaff)Payload;
				return skillStaff.ForecastedIncomingDemand;
			}
			set
			{
				SkillStaff skillStaff = (SkillStaff)Payload;
				skillStaff.ForecastedIncomingDemand = value;
			}
		}

		public SkillStaffPeriodEx(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData) : base(period, taskData, serviceAgreementData)
		{
		}

		public new void SetCalculatedResource65(double resources)
		{
			SkillStaff skillStaff = (SkillStaff)Payload;
			skillStaff.CalculatedResource = resources;

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
			var sa = TimeSpan.FromSeconds(AnsweredWithinSeconds);

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
		
		public override void PickResources65()
		{
			//_estimatedServiceLevel = null;
			//_estimatedServiceLevelShrinkage = null;

			SkillStaff thisSkillStaff = (SkillStaff) Payload;

			if (SortedSegmentCollection.Count == 1)
			{
				thisSkillStaff.BookedAgainstIncomingDemand65 = thisSkillStaff.CalculatedResource;
				SortedSegmentCollection[0].BookedResource65 = thisSkillStaff.CalculatedResource;
				_bookedResource65 = _segmentInThisCollection.Sum(x => x.BookedResource65);
			}
			else
			{
				var forecastIncomingDemand = thisSkillStaff.ForecastedIncomingDemand;

				thisSkillStaff.BookedAgainstIncomingDemand65 = 0;

				foreach (var xSegment in SortedSegmentCollection)
				{
					((SkillStaffPeriod) xSegment.BelongsToY)._bookedResource65 -= xSegment.BookedResource65;
					xSegment.BookedResource65 = 0;

					var diff = forecastIncomingDemand - thisSkillStaff.BookedAgainstIncomingDemand65;
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

						thisSkillStaff.BookedAgainstIncomingDemand65 += diff;
						xSegment.BookedResource65 = diff;
						((SkillStaffPeriod) xSegment.BelongsToY)._bookedResource65 += xSegment.BookedResource65;
					}
				}
			}
		}
	}
}