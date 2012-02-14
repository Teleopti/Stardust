
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{

    public class ResourceOptimizationHelper : IResourceOptimizationHelper
    {
        private readonly ISchedulingResultStateHolder _stateHolder;
    	private readonly IOccupiedSeatCalculator _occupiedSeatCalculator;
    	private readonly INonBlendSkillCalculator _nonBlendSkillCalculator;

    	public ResourceOptimizationHelper(ISchedulingResultStateHolder stateHolder, 
            IOccupiedSeatCalculator occupiedSeatCalculator, 
            INonBlendSkillCalculator nonBlendSkillCalculator)
		{
			_stateHolder = stateHolder;
			_occupiedSeatCalculator = occupiedSeatCalculator;
    		_nonBlendSkillCalculator = nonBlendSkillCalculator;
		}

        public void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool considerShortBreaks)
        {
            ResourceCalculateDate(localDate, useOccupancyAdjustment, true, considerShortBreaks);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "Teleopti.Interfaces.Domain.DateOnly.ToShortDateString")]
        private void ResourceCalculateDate(DateOnly localDate, bool useOccupancyAdjustment, bool calculateMaxSeatsAndNonBlend, bool considerShortBreaks)
        {
            if (_stateHolder.TeamLeaderMode)
                return;

            if(_stateHolder.SkipResourceCalculation)
                return;

            using (PerformanceOutput.ForOperation("ResourceCalculate " + localDate.ToShortDateString()))
            {
                var extractor = new ScheduleProjectionExtractor();
                IList<IVisualLayerCollection> relevantProjections;

                relevantProjections = extractor.CreateRelevantProjectionWithScheduleList(_stateHolder.Schedules,
                                                                                         TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(localDate.AddDays(-1), localDate.AddDays(1)));

                resourceCalculateDate(relevantProjections, localDate, useOccupancyAdjustment, calculateMaxSeatsAndNonBlend, considerShortBreaks);
            }
            
        }

        private static DateTimePeriod GetPeriod(DateOnly localDate)
        {
            DateTime currentStart = localDate;
            DateTime currentEnd = currentStart.AddDays(1).AddTicks(-1);
            return TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(currentStart, currentEnd);
        }

        private void resourceCalculateDate(IList<IVisualLayerCollection> relevantProjections, DateOnly localDate, bool useOccupancyAdjustment, bool calculateMaxSeatsAndNonBlend, bool considerShortBreaks)
        {
            var timePeriod = GetPeriod(localDate);
            var ordinarySkills = new List<ISkill>();
            foreach (var skill in _stateHolder.Skills)
            {
                if(skill.SkillType.ForecastSource != ForecastSource.NonBlendSkill && skill.SkillType.ForecastSource != ForecastSource.MaxSeatSkill)
                    ordinarySkills.Add(skill);
            }
            ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods;

            relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);

            var schedulingResultService = new SchedulingResultService(relevantSkillStaffPeriods, _stateHolder.Skills, relevantProjections, useOccupancyAdjustment);

            schedulingResultService.SchedulingResult(timePeriod);

            if(considerShortBreaks)
            {
                var periodDistributionService = new PeriodDistributionService(relevantProjections, 5);
                periodDistributionService.CalculateDay(relevantSkillStaffPeriods);
            }

            if (calculateMaxSeatsAndNonBlend)
            {
				ordinarySkills = new List<ISkill>();
				foreach (var skill in _stateHolder.Skills)
				{
					if (skill.SkillType.ForecastSource == ForecastSource.MaxSeatSkill)
						ordinarySkills.Add(skill);
				}
				relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);
                _occupiedSeatCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods);

				ordinarySkills = new List<ISkill>();
				foreach (var skill in _stateHolder.Skills)
				{
					if (skill.SkillType.ForecastSource == ForecastSource.NonBlendSkill)
						ordinarySkills.Add(skill);
				}
				relevantSkillStaffPeriods = CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, ordinarySkills, timePeriod);
                _nonBlendSkillCalculator.Calculate(localDate, relevantProjections, relevantSkillStaffPeriods, false);
            }

        _stateHolder.OnResourcesChanged(new List<DateOnly> {localDate});
            
        }

        ///// <summary>
        ///// Creates a skill-skillday dictionary from the inner skilldays for a specified date and a specified list of skills.
        ///// </summary>
        ///// <param name="keyPeriod">The key period.</param>
        ///// <returns></returns>
        ////private ISkillSkillStaffPeriodExtendedDictionary createSkillSkillStaffDictionary(DateTimePeriod keyPeriod)
        ////{
        ////    return CreateSkillSkillStaffDictionaryOnSkills(_stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary,
        ////                                                   _stateHolder.Skills, keyPeriod);
        ////}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public ISkillSkillStaffPeriodExtendedDictionary CreateSkillSkillStaffDictionaryOnSkills(ISkillSkillStaffPeriodExtendedDictionary skillStaffPeriodDictionary, IList<ISkill> skills, DateTimePeriod keyPeriod)
        {
            var result = new SkillSkillStaffPeriodExtendedDictionary();
            foreach (var skillPair in skillStaffPeriodDictionary)
            {
                if (!skills.Contains(skillPair.Key))
                    continue;
                ISkillStaffPeriodDictionary skillStaffDictionary = skillPair.Value;

                var skillStaffPeriodDictionaryToReturn = new SkillStaffPeriodDictionary(skillPair.Key);
                foreach (var skillStaffPeriod in skillStaffDictionary)
                {
                    if (!skillStaffPeriod.Key.Intersect(keyPeriod)) continue;
                    skillStaffPeriodDictionaryToReturn.Add(skillStaffPeriod.Key, skillStaffPeriod.Value);
                }
                result.Add(skillPair.Key, skillStaffPeriodDictionaryToReturn);
            }

            return result;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public void AddResourcesToNonBlendAndMaxSeat(IMainShift mainShift, IPerson person, DateOnly dateOnly)
        {
            var virtualSchedulePeriod = person.VirtualSchedulePeriod(dateOnly);
            if (!virtualSchedulePeriod.IsValid)
                return;

            var layerCollection = mainShift.ProjectionService().CreateProjection();

            var timePeriod = layerCollection.Period();

            if (!timePeriod.HasValue)
                return;

            // just a workaround for now so we get as Person attached to the collection
            var layers = layerCollection.ToList();
            layerCollection = new VisualLayerCollection(person, layers, new ProjectionPayloadMerger());

            var personPeriod = person.Period(dateOnly);
            //var personPeriod = virtualSchedulePeriod.PersonPeriod;
            var skills = new List<ISkill>();
            foreach (var personSkill in personPeriod.PersonNonBlendSkillCollection)
            {
                skills.Add(personSkill.Skill);
            }
            var relevantSkillStaffPeriods =
                CreateSkillSkillStaffDictionaryOnSkills(
                    _stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, skills, timePeriod.Value);

            _nonBlendSkillCalculator.Calculate(dateOnly,new List<IVisualLayerCollection>{layerCollection},relevantSkillStaffPeriods, true);

            skills.Clear();
            foreach (var personSkill in personPeriod.PersonMaxSeatSkillCollection)
            {
                skills.Add(personSkill.Skill);
            }
            relevantSkillStaffPeriods =
                CreateSkillSkillStaffDictionaryOnSkills(
                    _stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary, skills, timePeriod.Value);
            _occupiedSeatCalculator.Calculate(dateOnly,new List<IVisualLayerCollection>{layerCollection}, relevantSkillStaffPeriods);
        }
    }
}
