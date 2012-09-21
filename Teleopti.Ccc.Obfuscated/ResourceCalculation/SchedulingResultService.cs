﻿using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
    public class SchedulingResultService : ISchedulingResultService
    {
        private readonly ISkillSkillStaffPeriodExtendedDictionary _relevantSkillStaffPeriods;
        private readonly IList<ISkill> _allSkills;
        private readonly IList<IVisualLayerCollection> _relevantProjections;
    	private readonly ISingleSkillLoadedDecider _singleSkillLoadedDecider;
    	private readonly ISingleSkillCalculator _singleSkillCalculator;
    	private readonly bool _useOccupancyAdjustment;

		///// <summary>
		///// Initializes a new instance of the <see cref="SchedulingResultService"/> class.
		///// </summary>
		///// <param name="stateHolder">The state holder.</param>
		///// <remarks>
		///// Created by: tamasb
		///// Created date: 2008-11-03
		///// </remarks>
		//public SchedulingResultService(ISchedulingResultStateHolder stateHolder)
		//    : this(stateHolder, stateHolder.Skills, true)
		//{
		//}

       
        public SchedulingResultService(ISchedulingResultStateHolder stateHolder, 
			IList<ISkill> allSkills,
			ISingleSkillLoadedDecider singleSkillLoadedDecider,
			ISingleSkillCalculator singleSkillCalculator,
			bool useOccupancyAdjustment)
        {
            _useOccupancyAdjustment = useOccupancyAdjustment;
            _relevantSkillStaffPeriods = stateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
            _relevantProjections = createRelevantProjectionList(stateHolder.Schedules);
            _allSkills = allSkills;
			_singleSkillLoadedDecider = singleSkillLoadedDecider;
			_singleSkillCalculator = singleSkillCalculator;
        }

       
        public SchedulingResultService(ISkillSkillStaffPeriodExtendedDictionary relevantSkillStaffPeriods, 
			IList<ISkill> allSkills, 
			IList<IVisualLayerCollection> relevantProjections,
			ISingleSkillLoadedDecider singleSkillLoadedDecider,
			ISingleSkillCalculator singleSkillCalculator,
			bool useOccupancyAdjustment)
        {
            _relevantSkillStaffPeriods = relevantSkillStaffPeriods;
            _allSkills = allSkills;
            _relevantProjections = relevantProjections;
        	_singleSkillLoadedDecider = singleSkillLoadedDecider;
        	_singleSkillCalculator = singleSkillCalculator;
        	_useOccupancyAdjustment = useOccupancyAdjustment;
        }

        public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult()
        {
            DateTimePeriod? periodToRecalculate = null;

            foreach (var projection in _relevantProjections)
            {
                DateTimePeriod? projectedPeriod = projection.Period();
                if (projectedPeriod.HasValue)
                {
                    if (!periodToRecalculate.HasValue)
                        periodToRecalculate = projectedPeriod.Value;
                    else
                        periodToRecalculate = periodToRecalculate.Value.MaximumPeriod(projectedPeriod.Value);
                }
            }

            if (!periodToRecalculate.HasValue)
                return _relevantSkillStaffPeriods;

            DateTimePeriod? relevantPeriod = _relevantSkillStaffPeriods.Period();
            if (!relevantPeriod.HasValue)
                return _relevantSkillStaffPeriods;

            periodToRecalculate = relevantPeriod.Value.Intersection(periodToRecalculate.Value);

            if (!periodToRecalculate.HasValue)
                return _relevantSkillStaffPeriods;

            return SchedulingResult(periodToRecalculate.Value);
        }

        public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate)
        {
            return SchedulingResult(periodToRecalculate, true);
        }

        public ISkillSkillStaffPeriodExtendedDictionary SchedulingResult(DateTimePeriod periodToRecalculate, bool emptyCache)
        {
            if (_allSkills.Count == 0)
                return _relevantSkillStaffPeriods;

        	var period = periodToRecalculate.ToDateOnlyPeriod(TeleoptiPrincipal.Current.Regional.TimeZone);
            var datePeriod = new DateOnlyPeriod(period.StartDate.AddDays(-1),period.EndDate.AddDays(1));
            IAffectedPersonSkillService personSkillService = new AffectedPersonSkillService(datePeriod, _allSkills);

			if(_singleSkillLoadedDecider.IsSingleSkill(_allSkills))
			{
				_singleSkillCalculator.Calculate(_relevantProjections, _relevantSkillStaffPeriods);
			}
            var rc = new ScheduleResourceOptimizer(_relevantProjections, _relevantSkillStaffPeriods,
                                                                         personSkillService, emptyCache, new ActivityDivider());
            rc.Optimize(periodToRecalculate, _useOccupancyAdjustment);

            return _relevantSkillStaffPeriods;
        }

        private static IList<IVisualLayerCollection> createRelevantProjectionList(IScheduleDictionary scheduleDictionary)
        {
            var extractor = new ScheduleProjectionExtractor();
            return extractor.CreateRelevantProjectionList(scheduleDictionary);
        }
    }
}