using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
    public class NewBusinessRuleCollection : Collection<INewBusinessRule>, INewBusinessRuleCollection
    {
        private CultureInfo _culture = Thread.CurrentThread.CurrentUICulture;
        private NewBusinessRuleCollection()
        {
            //put mandatory here
					Add(new DataPartOfAgentDay());
        }

        public static INewBusinessRuleCollection Minimum()
        {
        	return new NewBusinessRuleCollection();
        }


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static INewBusinessRuleCollection All(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
		    IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
		    IDayOffMaxFlexCalculator dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
		    var ensureWeeklyRestRule = new EnsureWeeklyRestRule(workTimeStartEndExtractor, dayOffMaxFlexCalculator);
            var ret = new NewBusinessRuleCollection
                          {
                              new NewShiftCategoryLimitationRule(
                                  new ShiftCategoryLimitationChecker(schedulingResultStateHolder),
                                  new VirtualSchedulePeriodExtractor()),
                              new WeekShiftCategoryLimitationRule(
                                  new ShiftCategoryLimitationChecker(schedulingResultStateHolder),
                                  new VirtualSchedulePeriodExtractor(), new WeeksFromScheduleDaysExtractor()),
                              new NewNightlyRestRule(new WorkTimeStartEndExtractor()),
                              new NewMaxWeekWorkTimeRule(
                                  new WeeksFromScheduleDaysExtractor()),
                              new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(), new PersonWeekVoilatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(),new VerifyWeeklyRestAroundDayOffSpecification(),ensureWeeklyRestRule )),
                              new NewDayOffRule(new WorkTimeStartEndExtractor()),
                              new NewPersonAccountRule(schedulingResultStateHolder, schedulingResultStateHolder.AllPersonAccounts)

							  
							  //This one takes to long time tu run first time when caches are empty, so put on hold for now
							  //new NewLegalStateRule(
							  //    new ScheduleMatrixListCreator(schedulingResultStateHolder),
							  //    schedulingResultStateHolder.Schedules,
							  //    new WorkShiftMinMaxLengthCalculatorFactory())
                          };

			if(!schedulingResultStateHolder.TeamLeaderMode)
				ret.Add(new OpenHoursRule(schedulingResultStateHolder));
			{
				
			}
            return ret;
        }

        public IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
        {
            var responseList = new List<IBusinessRuleResponse>();
            using (new UICultureContext(_culture))
            {
                foreach (var rule in this)
                {
                    IEnumerable<IBusinessRuleResponse> retList = rule.Validate(rangeClones, scheduleDays);
                    responseList.AddRange(retList);
                }
            }
            return responseList;
        }

        public void Remove(IBusinessRuleResponse businessRuleResponseToOverride)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var bu = this[i];

                if (businessRuleResponseToOverride.TypeOfRule.Equals(bu.GetType()))
                {
                    if(!bu.IsMandatory)
                        bu.HaltModify = false;

                    return;
                }
            }
        }

        public void Remove(Type businessRuleType)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var bu = this[i];

                if (businessRuleType.Equals(bu.GetType()))
                {
                    if (!bu.IsMandatory)
                        bu.HaltModify = false;

                    return;
                }
            }
        }

        public INewBusinessRule Item(Type businessRuleType)
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                var bu = this[i];

                if (businessRuleType.Equals(bu.GetType()))
                {
                    return bu;
                }
            }

            return null;
        }

        public void SetUICulture(CultureInfo cultureInfo)
        {
            _culture = cultureInfo;
        }

        public CultureInfo UICulture
        {
            get { return _culture; }
        }

        protected override void RemoveItem(int index)
        {
            var rule = this[index];
            if (!rule.IsMandatory)
                rule.HaltModify = false;
        }

        protected override void ClearItems()
        {
            for (var i = Count - 1; i >= 0; i--)
            {
                RemoveItem(i);
            }
        }

		public static INewBusinessRuleCollection AllForDelete(ISchedulingResultStateHolder schedulingResultStateHolder)
		{

			var ret = All(schedulingResultStateHolder);

			foreach (INewBusinessRule rule in ret)
			{
				rule.HaltModify = false;
			}

			return ret;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static INewBusinessRuleCollection AllForScheduling(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
            INewBusinessRuleCollection ret;
            if(schedulingResultStateHolder.UseValidation)
                ret = All(schedulingResultStateHolder);
            else
            {
                ret = MinimumAndPersonAccount(schedulingResultStateHolder);
            }
			foreach (INewBusinessRule rule in ret)
			{
				rule.HaltModify = false;
			}
			return ret;
		}

		public static INewBusinessRuleCollection MinimumAndPersonAccount(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			if (schedulingResultStateHolder == null)
				return null;
			var ret = new NewBusinessRuleCollection
                          {
                              new NewPersonAccountRule(schedulingResultStateHolder, schedulingResultStateHolder.AllPersonAccounts)
                          };
			return ret;
		}
    }
}