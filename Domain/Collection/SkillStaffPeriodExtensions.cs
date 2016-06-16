using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Collection
{
	public static class SkillStaffPeriodExtensions
	{
		public static double ResourceLoggonOnDiff(this ISkillStaffPeriod skillStaffPeriod)
		{
			return skillStaffPeriod.CalculatedResource - skillStaffPeriod.CalculatedLoggedOn;
		}

		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillStaffPeriodDictionary skillStaffPeriodDictionary, DateTimePeriod period, int absoluteDiffIfNoSkillStaffPeriod)
		{
			ISkillStaffPeriod skillStaffPeriod;
			return skillStaffPeriodDictionary.TryGetValue(period, out skillStaffPeriod) ? 
				skillStaffPeriod :
				createNullObject(absoluteDiffIfNoSkillStaffPeriod);
		}

		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillSkillStaffPeriodExtendedDictionary skillSkillStaffPeriodExtendedDictionary, ISkill skill, DateTimePeriod period, int absoluteDiffIfNoSkillStaffPeriod)
		{
			ISkillStaffPeriodDictionary skillStaffPeriodDictionary;
			return skillSkillStaffPeriodExtendedDictionary.TryGetValue(skill, out skillStaffPeriodDictionary) ?
				SkillStaffPeriodOrDefault(skillStaffPeriodDictionary, period, absoluteDiffIfNoSkillStaffPeriod) : 
				createNullObject(absoluteDiffIfNoSkillStaffPeriod);
		}

		public static ISkillStaffPeriod SkillStaffPeriodOrDefault(this ISkillStaffPeriodHolder skillStaffPeriodHolder, ISkill skill, DateTimePeriod period, int absoluteDiffIfNoSkillStaffPeriod)
		{
			return SkillStaffPeriodOrDefault(skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, skill, period, absoluteDiffIfNoSkillStaffPeriod);
		}

		public static void AddResources(this ISkillStaffPeriod skillStaffPeriod, double resourceToAdd)
		{
			skillStaffPeriod.SetCalculatedResource65(skillStaffPeriod.CalculatedResource + resourceToAdd);
		}


		//behövs period?
		private static ISkillStaffPeriod createNullObject(int absoluteDiffIfNoSkillStaffPeriod)
		{
			return new skillStaffPeriodNullObject(absoluteDiffIfNoSkillStaffPeriod);
		}

		private class skillStaffPeriodNullObject : ISkillStaffPeriod
		{
			public skillStaffPeriodNullObject(double absoluteDifference)
			{
				AbsoluteDifference = absoluteDifference;
			}

			public DateTimePeriod Period { get; }
			public ISkillStaff Payload { get; }
			public object Clone()
			{
				throw new NotImplementedException();
			}

			public ILayer<ISkillStaff> NoneEntityClone()
			{
				throw new NotImplementedException();
			}

			public ILayer<ISkillStaff> EntityClone()
			{
				throw new NotImplementedException();
			}

			public IList<ISkillStaffSegmentPeriod> SortedSegmentCollection { get; }
			public IList<ISkillStaffSegmentPeriod> SegmentInThisCollection { get; }
			public double RelativeDifference { get; }
			public double RelativeDifferenceForDisplayOnly { get; }
			public double RelativeBoostedDifferenceForDisplayOnly { get; set; }
			public double RelativeDifferenceIncoming { get; }
			public double AbsoluteDifference { get; }
			public double ForecastedDistributedDemand { get; }
			public double ForecastedDistributedDemandWithShrinkage { get; }
			public bool IsAvailable { get; set; }
			public IStatisticTask StatisticTask { get; set; }
			public IActiveAgentCount ActiveAgentCount { get; set; }
			public void SetCalculatedResource65(double value)
			{
			}

			public void PickResources65()
			{
				throw new NotImplementedException();
			}

			public TimeSpan ForecastedIncomingDemand()
			{
				throw new NotImplementedException();
			}

			public TimeSpan ForecastedIncomingDemandWithShrinkage()
			{
				throw new NotImplementedException();
			}

			public void CalculateStaff(IList<ISkillStaffPeriod> periods)
			{
				throw new NotImplementedException();
			}

			public void CalculateStaff()
			{
				throw new NotImplementedException();
			}

			public ISkillStaffPeriod IntersectingResult(DateTimePeriod period)
			{
				throw new NotImplementedException();
			}

			public void Reset()
			{
				throw new NotImplementedException();
			}

			public double IntraIntervalDeviation { get; }
			public double IntraIntervalRootMeanSquare { get; }
			public double ScheduledAgentsIncoming { get; }
			public double IncomingDifference { get; }
			public double FStaff { get; }
			public IStaffingCalculatorServiceFacade StaffingCalculatorService { get; }
			public double BookedResource65 { get; }
			public double CalculatedLoggedOn { get; }
			public double ScheduledHours()
			{
				throw new NotImplementedException();
			}

			public double CalculatedResource { get; }
			public TimeSpan FStaffTime()
			{
				throw new NotImplementedException();
			}

			public double FStaffHours()
			{
				throw new NotImplementedException();
			}

			public double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(bool shiftValueMode)
			{
				throw new NotImplementedException();
			}

			public Percent EstimatedServiceLevel { get; }
			public Percent ActualServiceLevel { get; }
			public IPeriodDistribution PeriodDistribution { get; }
			public double AbsoluteDifferenceMinStaffBoosted()
			{
				throw new NotImplementedException();
			}

			public double AbsoluteDifferenceMaxStaffBoosted()
			{
				throw new NotImplementedException();
			}

			public double AbsoluteDifferenceBoosted()
			{
				throw new NotImplementedException();
			}

			public double RelativeDifferenceMinStaffBoosted()
			{
				throw new NotImplementedException();
			}

			public double RelativeDifferenceMaxStaffBoosted()
			{
				throw new NotImplementedException();
			}

			public double RelativeDifferenceBoosted()
			{
				throw new NotImplementedException();
			}

			public IList<ISkillStaffPeriodView> Split(TimeSpan periodLength)
			{
				throw new NotImplementedException();
			}

			public void SetDistributionValues(IPopulationStatisticsCalculatedValues calculatedValues,
				IPeriodDistribution periodDistribution)
			{
				throw new NotImplementedException();
			}

			public void ClearIntraIntervalDistribution()
			{
				throw new NotImplementedException();
			}

			public void SetSkillDay(ISkillDay skillDay)
			{
				throw new NotImplementedException();
			}

			public ISkillDay SkillDay { get; }
			public Percent EstimatedServiceLevelShrinkage { get; }
			public bool HasIntraIntervalIssue { get; set; }
			public double IntraIntervalValue { get; set; }
			public IList<int> IntraIntervalSamples { get; set; }
		}
	}
}