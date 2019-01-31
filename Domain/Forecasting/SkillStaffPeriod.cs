using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Forecasting
{
	/// <summary>
	/// Holds data needed to calculate staffing needs (activity start, end, length, occupancy etc.).
	/// </summary>
	public class SkillStaffPeriod : Layer<ISkillStaff>, ISkillStaffPeriod, IAggregateSkillStaffPeriod,
		IShovelResourceDataForInterval
	{
		protected internal readonly SortedList<DateTime, ISkillStaffSegmentPeriod> _sortedSegmentCollection;
		protected internal IList<ISkillStaffSegmentPeriod> _segmentInThisCollection;
		private bool _isAggregate;
		private double _aggregatedFStaff;
		private double _aggregatedCalculatedResources;
		private Percent? _estimatedServiceLevel;
		private Percent? _estimatedServiceLevelShrinkage;
		private IPeriodDistribution _periodDistribution;

		private readonly object Locker = new object();

		//protected internal SkillStaffPeriod()
		//{
		//	_sortedSegmentCollection = new SortedList<DateTime, ISkillStaffSegmentPeriod>();
		//	_segmentInThisCollection = new List<ISkillStaffSegmentPeriod>();
		//}
		
		public SkillStaffPeriod(DateTimePeriod period, ITask taskData, ServiceAgreement serviceAgreementData) : base(
			new SkillStaff(taskData, serviceAgreementData), period)
		{
			AggregatedStaffingThreshold = StaffingThreshold.Ok;
			_sortedSegmentCollection = new SortedList<DateTime, ISkillStaffSegmentPeriod>();
			_segmentInThisCollection = new List<ISkillStaffSegmentPeriod>();
			IntraIntervalSamples = new List<int>();
		}

		public IStatisticTask StatisticTask { get; set; }

		public IActiveAgentCount ActiveAgentCount { get; set; }

		public double IntraIntervalDeviation { get; private set; }

		public double RelativeBoostedDifferenceForDisplayOnly { get; set; }

		public bool HasIntraIntervalIssue { get; set; }

		public double IntraIntervalValue { get; set; }

		public IList<int> IntraIntervalSamples { get; set; }

		public DateTimePeriod DateTimePeriod => Period;

		public double ScheduledAgentsIncoming =>
			Payload.BookedAgainstIncomingDemand65 + (Payload.CalculatedResource - BookedResource65);

		public double IncomingDifference => ScheduledAgentsIncoming - Payload.ForecastedIncomingDemand;

		protected internal double _bookedResource65;

		public double BookedResource65
		{
			get
			{
				lock (Locker)
				{
					return _bookedResource65;
				}
			}
		}

		public double CalculatedLoggedOn
		{
			get
			{
				lock (Locker)
				{
					if (_isAggregate)
						return ((IAggregateSkillStaffPeriod) this).AggregatedCalculatedLoggedOn;

					return Payload.CalculatedLoggedOn;
				}
			}
		}

		public double AbsoluteDifferenceScheduledHeadsAndMinMaxHeads(bool shiftValueMode)
		{
			if (Payload.SkillPersonData.MinimumPersons > 0 &&
				Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
				return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons;
			if (!shiftValueMode && Payload.SkillPersonData.MaximumPersons > 0 &&
				Payload.CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
				return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons;
			if (shiftValueMode && Payload.SkillPersonData.MaximumPersons > 0 &&
				Payload.CalculatedLoggedOn >= Payload.SkillPersonData.MaximumPersons)
				return Payload.CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons + 1;
			return 0;
		}

		public Percent EstimatedServiceLevel
		{
			get
			{
				lock (Locker)
				{
					if (_isAggregate)
						return ((IAggregateSkillStaffPeriod) this).AggregatedEstimatedServiceLevel;

					if (!_estimatedServiceLevel.HasValue)
						CalculateEstimatedServiceLevel();

					return _estimatedServiceLevel.GetValueOrDefault(new Percent(0));
				}
			}
		}

		public Percent EstimatedServiceLevelShrinkage
		{
			get
			{
				lock (Locker)
				{
					if (_isAggregate)
						return ((IAggregateSkillStaffPeriod) this).AggregatedEstimatedServiceLevelShrinkage;

					if (!_estimatedServiceLevelShrinkage.HasValue)
						CalculateEstimatedServiceLevel();

					return _estimatedServiceLevelShrinkage.GetValueOrDefault(new Percent(0));
				}
			}
		}

		public Percent ActualServiceLevel
		{
			get
			{
				if (StatisticTask == null || StatisticTask.StatCalculatedTasks == 0)
					return new Percent(double.NaN);
				//Never over 100%:
				return StatisticTask.StatAnsweredTasksWithinSL >= StatisticTask.StatCalculatedTasks
					? new Percent(1)
					: new Percent(StatisticTask.StatAnsweredTasksWithinSL / StatisticTask.StatCalculatedTasks);
			}
		}

		public double ScheduledHours()
		{
			return CalculatedResource * Period.ElapsedTime().TotalHours;
		}

		public double CalculatedResource
		{
			get
			{
				lock (Locker)
				{
					if (_isAggregate)
						return ((IAggregateSkillStaffPeriod) this).AggregatedCalculatedResource;

					return Payload.CalculatedResource;
				}
			}
		}

		public double FStaff
		{
			get
			{
				lock (Locker)
				{
					if (_isAggregate)
						return ((IAggregateSkillStaffPeriod) this).AggregatedFStaff;

					return _segmentInThisCollection.Sum(ySegment => ySegment.FStaff());
				}
			}
		}

		public TimeSpan FStaffTime()
		{
			return TimeSpan.FromMinutes(FStaff * Period.ElapsedTime().TotalMinutes);
		}

		public double FStaffHours()
		{
			return FStaffTime().TotalHours;
		}

		public IList<ISkillStaffSegmentPeriod> SortedSegmentCollection => _sortedSegmentCollection.Values;

		public IList<ISkillStaffSegmentPeriod> SegmentInThisCollection => _segmentInThisCollection;

		public double RelativeDifference => new DeviationStatisticData(FStaff, CalculatedResource).RelativeDeviation;

		public double RelativeDifferenceForDisplayOnly =>
			new DeviationStatisticData(FStaff, CalculatedResource).RelativeDeviationForDisplay;

		public double RelativeDifferenceIncoming
		{
			get
			{
				if (Payload.ForecastedIncomingDemand == 0 && ScheduledAgentsIncoming == 0)
					return 0;

				if (Payload.ForecastedIncomingDemand == 0)
					return double.NaN;

				return IncomingDifference / Payload.ForecastedIncomingDemand;
			}
		}

		public double AbsoluteDifference => CalculatedResource - FStaff;

		public double AbsoluteDifferenceMinStaffBoosted()
		{
			if (Payload.SkillPersonData.MinimumPersons > 0 && CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
				return (CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000 + AbsoluteDifference;

			return AbsoluteDifference;
		}

		public double AbsoluteDifferenceMaxStaffBoosted()
		{

			if (Payload.SkillPersonData.MaximumPersons > 0 && CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
				return (CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000 + AbsoluteDifference;

			return AbsoluteDifference;
		}

		public double AbsoluteDifferenceBoosted()
		{
			double ret = 0;
			if (Payload.SkillPersonData.MinimumPersons > 0 && CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
				ret = (CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000;
			if (Payload.SkillPersonData.MaximumPersons > 0 && CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
				ret = (CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000;

			return ret + AbsoluteDifference;
		}

		public double RelativeDifferenceMinStaffBoosted()
		{
			if (Payload.SkillPersonData.MinimumPersons > 0 &&
				Payload.CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
				return (Payload.CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000 + RelativeDifference;

			return RelativeDifference;

		}

		public double RelativeDifferenceMaxStaffBoosted()
		{
			if (Payload.SkillPersonData.MaximumPersons > 0 && CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
				return (CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000 + RelativeDifference;

			return RelativeDifference;
		}

		public double RelativeDifferenceBoosted()
		{
			double ret = 0;
			if (Payload.SkillPersonData.MinimumPersons > 0 && CalculatedLoggedOn < Payload.SkillPersonData.MinimumPersons)
				ret = (CalculatedLoggedOn - Payload.SkillPersonData.MinimumPersons) * 10000;
			if (Payload.SkillPersonData.MaximumPersons > 0 && CalculatedLoggedOn > Payload.SkillPersonData.MaximumPersons)
				ret = (CalculatedLoggedOn - Payload.SkillPersonData.MaximumPersons) * 10000;

			return ret + RelativeDifference;
		}

		public IList<ISkillStaffPeriodView> Split(TimeSpan periodLength)
		{
			var skillResolution = Period.ElapsedTime();
			if ( skillResolution < periodLength)
				throw new ArgumentOutOfRangeException(nameof(periodLength), periodLength,
					"You cannot split to a higher period length");

			if (skillResolution.TotalMinutes % periodLength.TotalMinutes > 0)
				throw new ArgumentOutOfRangeException(nameof(periodLength), periodLength,
					"You cannot split if you get a remaining time");

			var resolutionFactor = periodLength.TotalMinutes / skillResolution.TotalMinutes;

			var newPeriods = Period.Intervals(periodLength);
			return newPeriods.Select(newPeriod => (ISkillStaffPeriodView)new SkillStaffPeriodView
			{
				Period = newPeriod,
				CalculatedResource = CalculatedResource,
				ForecastedIncomingDemand = Payload.ForecastedIncomingDemand,
				ForecastedTasks = Payload.TaskData.Tasks * resolutionFactor,
				EstimatedServiceLevel = EstimatedServiceLevel,
				EstimatedServiceLevelShrinkage = EstimatedServiceLevelShrinkage,
				FStaff = FStaff,
				AverageHandlingTaskTime = Payload.TaskData.AverageHandlingTaskTime
			}).ToArray();
		}

		public void SetDistributionValues(PopulationStatisticsCalculatedValues calculatedValues,
			IPeriodDistribution periodDistribution)
		{
			IntraIntervalDeviation = calculatedValues.StandardDeviation;
			_periodDistribution = periodDistribution;
		}

		public void ClearIntraIntervalDistribution()
		{
			IntraIntervalDeviation = 0;
		}

		public void SetSkillDay(ISkillDay skillDay)
		{
			SkillDay = skillDay;
		}

		public ISkillDay SkillDay { get; private set; }

		public void CalculateEstimatedServiceLevel()
		{
			var parent = SkillDay;

			//Never over 100%, if demand = 0 then 100 if Email etc
			if (parent != null && parent.Skill.SkillType.ForecastSource != ForecastSource.InboundTelephony
							   && parent.Skill.SkillType.ForecastSource != ForecastSource.Retail
							   && parent.Skill.SkillType.ForecastSource != ForecastSource.Chat)
			{
				if (Payload.ForecastedIncomingDemand == 0)
				{
					_estimatedServiceLevel = new Percent(1);
					_estimatedServiceLevelShrinkage = new Percent(1);
					return;
				}

				if (ScheduledAgentsIncoming >= Payload.ForecastedIncomingDemand)
				{
					_estimatedServiceLevel = new Percent(1);
					_estimatedServiceLevelShrinkage = new Percent(1);
					return;
				}

				_estimatedServiceLevel = new Percent(ScheduledAgentsIncoming / Payload.ForecastedIncomingDemand);
				// this case the shrinkage is already calculated in ForecastedIncomingDemand, so the following is ok
				_estimatedServiceLevelShrinkage = EstimatedServiceLevel;
			}
			else
			{
				_estimatedServiceLevel = new Percent(StaffingCalculatorService.ServiceLevelAchievedOcc(
					ScheduledAgentsIncoming,
					Payload.ServiceAgreementData.ServiceLevel.Seconds,
					Payload.TaskData.Tasks,
					Payload.TaskData.AverageHandlingTaskTime.TotalSeconds,
					Period.ElapsedTime(),
					Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
					Payload.ForecastedIncomingDemandWithoutShrinkage,
					SkillDay.Skill.MaxParallelTasks,
					SkillDay.Skill.AbandonRate.Value));

				if (largeVolumes())
				{
					_estimatedServiceLevelShrinkage = new Percent(StaffingCalculatorService.ServiceLevelAchievedOcc(
						ScheduledAgentsIncoming,
						Payload.ServiceAgreementData.ServiceLevel.Seconds,
						Payload.TaskData.Tasks,
						Payload.TaskData.AverageHandlingTaskTime.TotalSeconds,
						Period.ElapsedTime(),
						Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
						Payload.ForecastedIncomingDemand,
						SkillDay.Skill.MaxParallelTasks,
						SkillDay.Skill.AbandonRate.Value));
				}
				else
				{
					var shrinkage = Payload.UseShrinkage ? 1 - Payload.Shrinkage.Value : 1;
					var scheduledAgentsIncomingWithShrinkage = ScheduledAgentsIncoming * shrinkage;
					_estimatedServiceLevelShrinkage = new Percent(StaffingCalculatorService.ServiceLevelAchievedOcc(
						scheduledAgentsIncomingWithShrinkage,
						Payload.ServiceAgreementData.ServiceLevel.Seconds,
						Payload.TaskData.Tasks,
						Payload.TaskData.AverageHandlingTaskTime.TotalSeconds,
						Period.ElapsedTime(),
						Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
						Payload.ForecastedIncomingDemandWithoutShrinkage,
						SkillDay.Skill.MaxParallelTasks,
						SkillDay.Skill.AbandonRate.Value));
				}
			}
		}

		private bool largeVolumes()
		{
			return ScheduledAgentsIncoming > 100;
		}

		public void SetCalculatedResource65(double value)
		{
			SkillStaff skillStaff = (SkillStaff) Payload;
			skillStaff.CalculatedResource = value;

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

		public void SetCalculatedLoggedOn(double loggedOn)
		{
			Payload.CalculatedLoggedOn = loggedOn;
		}

		public void ResetMultiskillMinOccupancy()
		{
			Payload.MultiskillMinOccupancy = null;
		}

		public DateTimePeriod CalculationPeriod => Period;

		public void SetCalculatedUsedSeats(double usedSeats)
		{
			Payload.CalculatedUsedSeats = usedSeats;
		}

		public TimeSpan ForecastedIncomingDemand()
		{
			return TimeSpan.FromMinutes(Payload.ForecastedIncomingDemand * Period.ElapsedTime().TotalMinutes);
		}

		public TimeSpan ForecastedIncomingDemandWithShrinkage()
		{
			return TimeSpan.FromMinutes(ForecastedIncomingDemand().TotalMinutes / (1 - Payload.Shrinkage.Value));
		}

		public virtual void CalculateStaff(IList<ISkillStaffPeriod> periods)
		{
			if (Payload.IsCalculated) return;

			double traffic = 0;
			double maxOcc = Payload.ServiceAgreementData.MaxOccupancy.Value;

			if (maxOcc == 0) maxOcc = 1;

			double minOcc =
				Payload.MultiskillMinOccupancy.GetValueOrDefault(Payload.ServiceAgreementData.MinOccupancy).Value;

			var maxParallel = SkillDay.Skill.MaxParallelTasks;
			var abandonRate = SkillDay.Skill.AbandonRate;
			var agentsAndOccupancy = new AgentsAndOccupancy
			{
				Agents = 0,
				Occupancy = 1
			};

			if (Payload.TaskData.Tasks == 0 && Payload.ServiceAgreementData.MinOccupancy.Value == 0)
			{
				traffic = 0;
			}
			else
			{
				if (!Payload.ManualAgents.HasValue && !Payload.NoneBlendDemand.HasValue)
				{
					agentsAndOccupancy = StaffingCalculatorService.AgentsUseOccupancy(
						Payload.ServiceAgreementData.ServiceLevel.Percent.Value,
						(int) Math.Round(Payload.ServiceAgreementData.ServiceLevel.Seconds),
						Payload.TaskData.Tasks,
						Payload.TaskData.AverageTaskTime.TotalSeconds + Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
						Period.ElapsedTime(),
						minOcc,
						maxOcc,
						maxParallel,
						abandonRate.Value);

					traffic = agentsAndOccupancy.Agents;
				}

			}

			var castedPayLoad = (SkillStaff) Payload;
			double demandWithoutEfficiency;
			if (!Payload.ManualAgents.HasValue && !Payload.NoneBlendDemand.HasValue)
			{
				castedPayLoad.ForecastedIncomingDemand = traffic;
				demandWithoutEfficiency = traffic;
			}
			else
			{
				if (Payload.ManualAgents.HasValue)
				{
					castedPayLoad.ForecastedIncomingDemand = Payload.ManualAgents.Value;
					demandWithoutEfficiency = Payload.ManualAgents.Value;
				}
				else
				{
					castedPayLoad.ForecastedIncomingDemand = Payload.NoneBlendDemand.Value;
					demandWithoutEfficiency = Payload.NoneBlendDemand.Value;
				}
			}
			
			castedPayLoad.CalculatedOccupancy = StaffingCalculatorService.Utilization(demandWithoutEfficiency,
				Payload.TaskData.Tasks,
				Payload.TaskData.AverageTaskTime.TotalSeconds +
				Payload.TaskData.AverageAfterTaskTime.TotalSeconds, Period.ElapsedTime(), maxParallel, agentsAndOccupancy.Occupancy);

			if (periods != null)
			{
				int thisIndex = periods.IndexOf(this);
				if (thisIndex == -1)
					throw new ArgumentException("List with skillstaffperiods must contain working skillstaffperiod");
				RemoveExistingSegments(periods, thisIndex);
				CreateSkillStaffSegments65(periods, thisIndex);
			}

			castedPayLoad.IsCalculated = true;
		}

		private void RemoveExistingSegments(IList<ISkillStaffPeriod> skillStaffPeriods, int currentIndex)
		{
			_sortedSegmentCollection.Clear();
			for (int i = currentIndex; i < skillStaffPeriods.Count; i++)
			{
				if (!(skillStaffPeriods[i] is SkillStaffPeriod period)) continue;
				var belongsToThis = period.SegmentInThisCollection.Where(x => x.BelongsTo.Equals(this)).ToArray();
				if (belongsToThis.Length == 0 && i > currentIndex)
					break; //Because we do this sequential, we can stop when no more distributed segments are found

				foreach (ISkillStaffSegmentPeriod segmentPeriod in belongsToThis)
				{
					period._bookedResource65 -= segmentPeriod.BookedResource65;
					period._segmentInThisCollection.Remove(segmentPeriod);
				}
			}
		}

		public void CalculateStaff()
		{
			CalculateStaff(new List<ISkillStaffPeriod> {this});
		}

		public static ISkillStaffPeriod Combine(IList<ISkillStaffPeriod> skillStaffPeriods)
		{
			if (skillStaffPeriods.Count < 1)
				return null;
			if (skillStaffPeriods.Count == 1)
				return (ISkillStaffPeriod) skillStaffPeriods[0].NoneEntityClone();

			DateTimePeriod period = skillStaffPeriods[0].Period;
			var firstElapsedTime = period.ElapsedTime();
			for (int index = 1; index < skillStaffPeriods.Count; index++)
			{
				TimeSpan elapsedTime = skillStaffPeriods[index].Period.ElapsedTime();
				if (elapsedTime != firstElapsedTime)
					InParameter.CheckTimeLimit(nameof(skillStaffPeriods), elapsedTime, 0);
			}

			double tasks = 0;
			double taskSeconds = 0;
			double afterTaskSeconds = 0;
			double serviceSeconds = 0;
			double servicePercent = 0;
			double minOcc = 0;
			double maxOcc = 0;
			double resource = 0;
			bool isAvail = false;
			bool useShrinkage = false;
			double forecastedIncomingHours = 0;

			foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriods)
			{
				tasks += skillStaffPeriod.Payload.TaskData.Tasks;
				taskSeconds += skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds *
							   skillStaffPeriod.Payload.TaskData.Tasks;
				afterTaskSeconds += skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds *
									skillStaffPeriod.Payload.TaskData.Tasks;
				serviceSeconds += skillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel.Seconds *
								  skillStaffPeriod.Payload.TaskData.Tasks;
				servicePercent += skillStaffPeriod.Payload.ServiceAgreementData.ServiceLevel.Percent.Value *
								  skillStaffPeriod.Payload.TaskData.Tasks;
				minOcc += skillStaffPeriod.Payload.ServiceAgreementData.MinOccupancy.Value *
						  skillStaffPeriod.Payload.TaskData.Tasks;
				maxOcc += skillStaffPeriod.Payload.ServiceAgreementData.MaxOccupancy.Value *
						  skillStaffPeriod.Payload.TaskData.Tasks;
				resource += skillStaffPeriod.Payload.CalculatedResource;
				if (!isAvail)
					isAvail = skillStaffPeriod.IsAvailable;
				useShrinkage = skillStaffPeriod.Payload.UseShrinkage;

				forecastedIncomingHours += skillStaffPeriod.Payload.ForecastedIncomingDemand;
			}

			var skillDay = skillStaffPeriods[0].SkillDay;
			if (tasks == 0)
			{
				var newPeriod = new SkillStaffPeriod(period, new Task(), ServiceAgreement.DefaultValues());
				newPeriod.Payload.ManualAgents = skillStaffPeriods[0].Payload.ManualAgents;
				if (newPeriod.Payload.ManualAgents.HasValue)
					((SkillStaff) newPeriod.Payload).ForecastedIncomingDemand = newPeriod.Payload.ManualAgents.Value;
				newPeriod.SetSkillDay(skillDay);
				return newPeriod;
			}

			Task retTask = new Task(tasks, TimeSpan.FromSeconds(taskSeconds / tasks),
				TimeSpan.FromSeconds(afterTaskSeconds / tasks));
			ServiceLevel retLevel = new ServiceLevel(new Percent(servicePercent / tasks), serviceSeconds / tasks);
			Percent retMinOcc = new Percent(minOcc / tasks);
			Percent retMaxOcc = new Percent(maxOcc / tasks);
			ServiceAgreement retAgr = new ServiceAgreement(retLevel, retMinOcc, retMaxOcc);

			var ret = new SkillStaffPeriod(period, retTask, retAgr) {IsAvailable = isAvail};
			ret.Payload.UseShrinkage = useShrinkage;
			ret.Payload.ManualAgents = skillStaffPeriods[0].Payload.ManualAgents;
			ret.SetCalculatedResource65(resource);
			((SkillStaff) ret.Payload).ForecastedIncomingDemand =
				skillStaffPeriods[0].Payload.ManualAgents ?? forecastedIncomingHours;
			ret.SetSkillDay(skillDay);

			return ret;
		}

		public double ForecastedDistributedDemand
		{
			get
			{
				lock (Locker)
				{
					return (from segmentPeriod in _segmentInThisCollection
						select segmentPeriod.BelongsTo
						into owner
						let segmentCount = owner.SortedSegmentCollection.Count
						where segmentCount > 0
						select owner.Payload.ForecastedIncomingDemand / segmentCount).Sum();
				}
			}
		}

		public double ForecastedDistributedDemandWithShrinkage =>
			ForecastedDistributedDemand * (1d + Payload.Shrinkage.Value);

		public bool IsAvailable { get; set; }

		public IStaffingCalculatorServiceFacade StaffingCalculatorService =>
			SkillDay.Skill.SkillType.StaffingCalculatorService;

		public void PickResources65()
		{
			_estimatedServiceLevel = null;
			_estimatedServiceLevelShrinkage = null;

			SkillStaff thisSkillStaff = (SkillStaff) Payload;

			if (hasSingleSegment())
			{
				thisSkillStaff.BookedAgainstIncomingDemand65 = thisSkillStaff.CalculatedResource;
				SortedSegmentCollection[0].BookedResource65 = thisSkillStaff.CalculatedResource;
				_bookedResource65 = _segmentInThisCollection.Sum(x => x.BookedResource65);
			}
			else
			{
				var forecastedIncomingDemand = thisSkillStaff.ForecastedIncomingDemand;

				thisSkillStaff.BookedAgainstIncomingDemand65 = 0;

				foreach (ISkillStaffSegmentPeriod xSegment in SortedSegmentCollection)
				{
					((SkillStaffPeriod) xSegment.BelongsToY)._bookedResource65 -= xSegment.BookedResource65;
					xSegment.BookedResource65 = 0;

					double diff = forecastedIncomingDemand - thisSkillStaff.BookedAgainstIncomingDemand65;
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

		private bool hasSingleSegment()
		{
			return SortedSegmentCollection.Count == 1;
		}

		public ISkillStaffPeriod IntersectingResult(DateTimePeriod period)
		{
			if (!Period.Intersect(period))
				return null;

			ISkillStaffPeriod skillStaffPeriod;
			if (period.Contains(Period))
			{
				skillStaffPeriod = new SkillStaffPeriod(period, Payload.TaskData,
					new ServiceAgreement(Payload.ServiceAgreementData.ServiceLevel,
						Payload.ServiceAgreementData.MinOccupancy,
						Payload.ServiceAgreementData.MaxOccupancy));
				skillStaffPeriod.Payload.Shrinkage = Payload.Shrinkage;
				skillStaffPeriod.Payload.SkillPersonData = Payload.SkillPersonData;
				return skillStaffPeriod;
			}

			DateTimePeriod intersection = Period.Intersection(period).Value;
			double intersectPercent = intersection.ElapsedTime().TotalSeconds /
									  Period.ElapsedTime().TotalSeconds;
			double newTasks = intersectPercent * Payload.TaskData.Tasks;
			ITask newTaskData = new Task(newTasks, Payload.TaskData.AverageTaskTime, Payload.TaskData.AverageAfterTaskTime);

			skillStaffPeriod = new SkillStaffPeriod(period, newTaskData,
				new ServiceAgreement(Payload.ServiceAgreementData.ServiceLevel,
					Payload.ServiceAgreementData.MinOccupancy,
					Payload.ServiceAgreementData.MaxOccupancy));
			skillStaffPeriod.Payload.Shrinkage = Payload.Shrinkage;
			return skillStaffPeriod;
		}

		private static void AddToLists(double traffToDistribute, ISkillStaffPeriod currentPeriod, ISkillStaffPeriod ourPeriod)
		{
			ISkillStaffSegment newSegment = new SkillStaffSegment(traffToDistribute);
			ISkillStaffSegmentPeriod newSegmentPeriod =
				new SkillStaffSegmentPeriod(ourPeriod, currentPeriod, newSegment, currentPeriod.Period);
			((SkillStaffPeriod) ourPeriod)._sortedSegmentCollection.Add(newSegmentPeriod.Period.StartDateTime, newSegmentPeriod);

			var skillStaffPeriod = (SkillStaffPeriod) currentPeriod;
			skillStaffPeriod._segmentInThisCollection.Add(newSegmentPeriod);
			skillStaffPeriod._bookedResource65 += newSegmentPeriod.BookedResource65;
		}

		protected internal void CreateSkillStaffSegments65(IList<ISkillStaffPeriod> sortedPeriods, int currentIndex)
		{
			ISkillStaffPeriod ourPeriod = sortedPeriods[currentIndex];
			TimeSpan sa = TimeSpan.FromSeconds(Payload.ServiceAgreementData.ServiceLevel.Seconds);

			for (; currentIndex < sortedPeriods.Count; currentIndex++)
			{
				ISkillStaffPeriod currentPeriod = sortedPeriods[currentIndex];
				TimeSpan currentLength = currentPeriod.Period.ElapsedTime();
				AddToLists(0, currentPeriod, ourPeriod);

				sa = sa.Add(-currentLength);

				if (sa <= TimeSpan.Zero) break;
			}
		}

		public void Reset()
		{
			Payload.Reset();
			_sortedSegmentCollection.Clear();
			_segmentInThisCollection = SegmentInThisCollection.Where(s => !s.BelongsTo.Equals(this)).ToList();
			_bookedResource65 = _segmentInThisCollection.Sum(x => x.BookedResource65);
			IsAvailable = false;
		}

		public override string ToString()
		{
			return $"{base.ToString()}, {Period}";
		}

		bool IAggregateSkillStaffPeriod.IsAggregate
		{
			get => _isAggregate;
			set => _isAggregate = value;
		}

		double IAggregateSkillStaffPeriod.AggregatedFStaff
		{
			get => _aggregatedFStaff;
			set => _aggregatedFStaff = value;
		}

		double IAggregateSkillStaffPeriod.AggregatedCalculatedLoggedOn => double.NaN;

		void IAggregateSkillStaffPeriod.CombineAggregatedSkillStaffPeriod(
			IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
		{
			if (_isAggregate && aggregateSkillStaffPeriod.IsAggregate)
			{
				_aggregatedFStaff += aggregateSkillStaffPeriod.AggregatedFStaff;
				_aggregatedCalculatedResources += aggregateSkillStaffPeriod.AggregatedCalculatedResource;
				AggregatedEstimatedServiceLevel = CombineEstimatedServiceLevel(aggregateSkillStaffPeriod);
				AggregatedEstimatedServiceLevelShrinkage = CombineEstimatedServiceLevelShrinkage(aggregateSkillStaffPeriod);
				AggregatedForecastedIncomingDemand += aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
				AggregatedMinMaxStaffAlarm = combineMinMaxStaffAlarm(aggregateSkillStaffPeriod);
				AggregatedStaffingThreshold = combineStaffingThreshold(aggregateSkillStaffPeriod);
			}
			else
			{
				throw new InvalidCastException("Both instances must be aggregated");
			}
		}

		private StaffingThreshold combineStaffingThreshold(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
		{
			int thisStatus = (int) AggregatedStaffingThreshold;
			int otherStatus = (int) aggregateSkillStaffPeriod.AggregatedStaffingThreshold;
			return (StaffingThreshold) Math.Max(thisStatus, otherStatus);
		}

		private MinMaxStaffBroken combineMinMaxStaffAlarm(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
		{
			if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
				return AggregatedMinMaxStaffAlarm;

			if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.Ok &&
				aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm != MinMaxStaffBroken.Ok)
				return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

			if (aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.BothBroken)
				return aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm;

			if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken &&
				aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken)
				return MinMaxStaffBroken.BothBroken;

			if (AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MaxStaffBroken &&
				aggregateSkillStaffPeriod.AggregatedMinMaxStaffAlarm == MinMaxStaffBroken.MinStaffBroken)
				return MinMaxStaffBroken.BothBroken;

			return AggregatedMinMaxStaffAlarm;
		}

		private Percent CombineEstimatedServiceLevel(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
		{
			double value = AggregatedForecastedIncomingDemand * AggregatedEstimatedServiceLevel.Value +
						   aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand *
						   aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevel.Value;
			double demand = AggregatedForecastedIncomingDemand +
							aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
			if (demand == 0)
				return new Percent(1);
			return new Percent(value / demand);
		}

		private Percent CombineEstimatedServiceLevelShrinkage(IAggregateSkillStaffPeriod aggregateSkillStaffPeriod)
		{
			double value = AggregatedForecastedIncomingDemand * AggregatedEstimatedServiceLevelShrinkage.Value +
						   aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand *
						   aggregateSkillStaffPeriod.AggregatedEstimatedServiceLevelShrinkage.Value;
			double demand = AggregatedForecastedIncomingDemand +
							aggregateSkillStaffPeriod.AggregatedForecastedIncomingDemand;
			if (demand == 0)
				return new Percent(1);
			return new Percent(value / demand);
		}

		double IAggregateSkillStaffPeriod.AggregatedCalculatedResource
		{
			get => _aggregatedCalculatedResources;
			set => _aggregatedCalculatedResources = value;
		}

		public Percent AggregatedEstimatedServiceLevel { get; set; }
		public Percent AggregatedEstimatedServiceLevelShrinkage { get; set; }

		public double AggregatedForecastedIncomingDemand { get; set; }

		public MinMaxStaffBroken AggregatedMinMaxStaffAlarm { get; set; }

		public StaffingThreshold AggregatedStaffingThreshold { get; set; }

		public IPeriodDistribution PeriodDistribution => _periodDistribution;

		public void AddResources(double resourcesToAdd)
		{
			var newValue = Math.Max(0, CalculatedResource + resourcesToAdd);
			SetCalculatedResource65(newValue);
		}
	}

	public class PopulationStatisticsCalculatedValues
	{
		public PopulationStatisticsCalculatedValues(double standardDeviation, double rootMeanSquare)
		{
			RootMeanSquare = rootMeanSquare;
			StandardDeviation = standardDeviation;
		}

		public double StandardDeviation { get; private set; }
		public double RootMeanSquare { get; private set; }
	}
}
