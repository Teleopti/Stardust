using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{

    /// <summary>
    /// Returns MinMax ContractTime for a date
    /// </summary>
    /// <remarks>
    /// Created by: henrika
    /// Created date: 2008-05-20
    /// </remarks>
    public class MinMaxContractData : IMinMaxContractTimeCalculator, IScheduleExtractor
    {
        private readonly IDictionary<DateTime, TimeSpan> _contractTimeDictionary = new Dictionary<DateTime, TimeSpan>();
        private readonly IRuleSetProjectionService _ruleSetProjectionService;
        private readonly IPerson _person;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinMaxContractData"/> class.
        /// </summary>
        /// <param name="ruleSetProjectionService">The rule set projection service.</param>
        /// <param name="person">The person.</param>
        /// <remarks>
        /// TODO: Remove and replace with schedulePart ctor
        /// Created by: henrika
        /// Created date: 2008-05-20
        /// </remarks>
        public MinMaxContractData(IRuleSetProjectionService ruleSetProjectionService, IPerson person)
        {
            InParameter.NotNull("person", person);
            _ruleSetProjectionService = ruleSetProjectionService;
            _person = person;
        }

		public MinMax<TimeSpan>? GetMinMaxContractTime(DateOnly workShiftDate, ISchedulingResultStateHolder resultStateHolder, ISchedulingOptions schedulingOptions)
        {
            InParameter.NotNull("SchedulingResultStateHolder", resultStateHolder);
            InParameter.NotNull("schedulingOptions", schedulingOptions);

            if (_contractTimeDictionary.ContainsKey(workShiftDate.Date))
                return new MinMax<TimeSpan>(_contractTimeDictionary[workShiftDate.Date],
                                            _contractTimeDictionary[workShiftDate.Date]);
            
            IPersonPeriod period = _person.Period(workShiftDate);
            if (period != null)
            {
                if (period.RuleSetBag != null && !((IDeleteTag)period.RuleSetBag).IsDeleted)
                {
                    var extractor = new RestrictionExtractor(resultStateHolder);
                    extractor.Extract(_person, workShiftDate);

                    IEffectiveRestriction restriction = extractor.CombinedRestriction(schedulingOptions);
                    if (restriction == null)
                        return null;

                    IWorkTimeMinMax ret = period.RuleSetBag.MinMaxWorkTime(_ruleSetProjectionService, workShiftDate, restriction);
                    if(ret == null)
                        return null;

                    return new MinMax<TimeSpan>(ret.WorkTimeLimitation.StartTime.Value, ret.WorkTimeLimitation.EndTime.Value) ;
                }

            }

            return null;
        }

        /// <summary>
        /// Uses the extractor to initialize the dictionary with complete data
        /// Loads up the scheduled contracttimes
        /// ContractTimes are sorted by the PersonAssignments StartDate
        /// </summary>
        /// <param name="schedulePart">The schedule part.</param>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-06-04
        /// </remarks>
        public void AddSchedulePart(IScheduleDay schedulePart)
        {
           //TODO henrika 2008-06-05 refactor, group by date
            _contractTimeDictionary.Clear(); 
            IVisualLayerCollection projectedLayerCollection = schedulePart.ProjectionService().CreateProjection();

            var contractItem = from v in projectedLayerCollection
                               where v.Payload.InContractTime
                               select
                                   new
                                       {
                                           DateTime = v.Period.StartDateTime.Date,
                                           ContractTime = v.Period.ElapsedTime()
                                       };
           
            foreach (var c in contractItem)
            {
                TimeSpan t;
                if (_contractTimeDictionary.TryGetValue(c.DateTime, out t))
                {
                    _contractTimeDictionary[c.DateTime] = c.ContractTime.Add(t);
                }
                else
                {
                    _contractTimeDictionary.Add(c.DateTime,c.ContractTime);
                }
            }
           
            foreach (PersonDayOff dayOff in schedulePart.PersonDayOffCollection())
            {
                DateTime date = dayOff.DayOff.Anchor.Date;
                
                if (_contractTimeDictionary.ContainsKey(date)) _contractTimeDictionary[date] = TimeSpan.Zero;
                else _contractTimeDictionary.Add(date,TimeSpan.Zero);
            }
        }
    }
}
