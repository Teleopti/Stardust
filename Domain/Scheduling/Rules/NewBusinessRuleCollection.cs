using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewBusinessRuleCollection : Collection<INewBusinessRule>, INewBusinessRuleCollection
	{
		#region Mapping between BusinessRule and BusinessRuleFlags

		private static readonly Dictionary<Type, BusinessRuleFlags> ruleAndFlagMapping
			= new Dictionary<Type, BusinessRuleFlags>
			{
				{
					typeof (DataPartOfAgentDay), BusinessRuleFlags.DataPartOfAgentDay
				},
				{
					typeof (MinWeeklyRestRule), BusinessRuleFlags.MinWeeklyRestRule
				},
				{
					typeof (MinWeekWorkTimeRule), BusinessRuleFlags.MinWeekWorkTimeRule
				},
				{
					typeof (NewDayOffRule), BusinessRuleFlags.NewDayOffRule
				},
				{
					typeof (NewMaxWeekWorkTimeRule), BusinessRuleFlags.NewMaxWeekWorkTimeRule
				},
				{
					typeof (NewNightlyRestRule), BusinessRuleFlags.NewNightlyRestRule
				},
				{
					typeof (NewPersonAccountRule), BusinessRuleFlags.NewPersonAccountRule
				},
				{
					typeof (NewShiftCategoryLimitationRule), BusinessRuleFlags.NewShiftCategoryLimitationRule
				},
				{
					typeof (NonMainShiftActivityRule), BusinessRuleFlags.NonMainShiftActivityRule
				},
				{
					typeof (OpenHoursRule), BusinessRuleFlags.OpenHoursRule
				},
				{
					typeof (WeekShiftCategoryLimitationRule), BusinessRuleFlags.WeekShiftCategoryLimitationRule
				},
				{
					typeof (SiteOpenHoursRule), BusinessRuleFlags.SiteOpenHoursRule
				},
				{
					typeof(ShiftTradeValidationFailedRule), BusinessRuleFlags.ShiftTradeValidationFailedRule
				}
			};

		private static readonly Dictionary<BusinessRuleFlags, string> flagAndRuleDescriptionMapping
			= new Dictionary<BusinessRuleFlags, string>
			{
				{
					BusinessRuleFlags.DataPartOfAgentDay, "NotAllowedChange"
				},
				{
					BusinessRuleFlags.MinWeeklyRestRule, "WeeklyRestTime"
				},
				{
					BusinessRuleFlags.MinWeekWorkTimeRule, "MinWeeklyWorkTime"
				},
				{
					BusinessRuleFlags.NewDayOffRule, "DayOff"
				},
				{
					BusinessRuleFlags.NewMaxWeekWorkTimeRule, "WeeklyWorkTime"
				},
				{
					BusinessRuleFlags.NewNightlyRestRule, "NightRest"
				},
				{
					BusinessRuleFlags.NewPersonAccountRule, "PersonAccount"
				},
				{
					BusinessRuleFlags.NewShiftCategoryLimitationRule, "ShiftCategory"
				},
				{
					BusinessRuleFlags.NonMainShiftActivityRule, "ShiftActivity"
				},
				{
					BusinessRuleFlags.OpenHoursRule, "SkillOpeningHours"
				},
				{
					BusinessRuleFlags.WeekShiftCategoryLimitationRule, "ShiftCategory"
				},
				{
					BusinessRuleFlags.SiteOpenHoursRule, "SiteOpeningHours"
				},
				{
					BusinessRuleFlags.ShiftTradeValidationFailedRule, "ShiftTradeSpecification"
				}
			};

		#endregion Mapping between BusinessRule and BusinessRuleFlags

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
					new ShiftCategoryLimitationChecker(() => schedulingResultStateHolder),
					new VirtualSchedulePeriodExtractor()),
				new WeekShiftCategoryLimitationRule(
					new ShiftCategoryLimitationChecker(() => schedulingResultStateHolder),
					new VirtualSchedulePeriodExtractor(), new WeeksFromScheduleDaysExtractor()),
				new NewNightlyRestRule(new WorkTimeStartEndExtractor()),
				new NewMaxWeekWorkTimeRule(
					new WeeksFromScheduleDaysExtractor()),
				new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(),
					new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(),
						new VerifyWeeklyRestAroundDayOffSpecification(), ensureWeeklyRestRule)),
				new NewDayOffRule(new WorkTimeStartEndExtractor()),
				new NewPersonAccountRule(schedulingResultStateHolder, schedulingResultStateHolder.AllPersonAccounts),
				new NotOverwriteLayerRule()

				//This one takes to long time tu run first time when caches are empty, so put on hold for now
				//new NewLegalStateRule(
				//    new ScheduleMatrixListCreator(schedulingResultStateHolder),
				//    schedulingResultStateHolder.Schedules,
				//    new WorkShiftMinMaxLengthCalculatorFactory())
			};

			if (schedulingResultStateHolder.UseMinWeekWorkTime)
				ret.Add(new MinWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));

			if (!schedulingResultStateHolder.TeamLeaderMode)
				ret.Add(new OpenHoursRule(schedulingResultStateHolder));

			return ret;
		}

		public static IEnumerable<string> GetRuleDescriptionsFromFlag(BusinessRuleFlags businessRuleFlags)
		{
			return (from kp in flagAndRuleDescriptionMapping
				where businessRuleFlags.HasFlag(kp.Key)
				select kp.Value).ToList();
		}

		public static BusinessRuleFlags GetFlagFromRules(IEnumerable<Type> ruleTypes)
		{
			var result = BusinessRuleFlags.None;
			foreach (var ruleType in ruleTypes)
			{
				if (ruleType == null || !ruleAndFlagMapping.ContainsKey(ruleType)) continue;
				result = result | ruleAndFlagMapping[ruleType];
			}

			return result;
		}

		public IEnumerable<IBusinessRuleResponse> CheckRules(IDictionary<IPerson, IScheduleRange> rangeClones,
			IEnumerable<IScheduleDay> scheduleDays)
		{
			var scheduleDayList = scheduleDays.ToList();
			var responseList = new List<IBusinessRuleResponse>();
			using (new UICultureContext(_culture))
			{
				foreach (var rule in this)
				{
					var retList = rule.Validate(rangeClones, scheduleDayList);
					responseList.AddRange(retList);
				}
			}
			return responseList;
		}

		public void DoNotHaltModify(IBusinessRuleResponse businessRuleResponseToOverride)
		{
			for (var i = Count - 1; i >= 0; i--)
			{
				var bu = this[i];
				if (businessRuleResponseToOverride.TypeOfRule != bu.GetType()) continue;

				if (!bu.IsMandatory) bu.HaltModify = false;

				return;
			}
		}

		public void DoNotHaltModify(Type businessRuleType)
		{
			for (var i = Count - 1; i >= 0; i--)
			{
				var bu = this[i];
				if (businessRuleType != bu.GetType()) continue;

				// Does it means that mandatory rules will never be removed?
				if (!bu.IsMandatory) bu.HaltModify = false;

				return;
			}
		}

		public INewBusinessRule Item(Type businessRuleType)
		{
			for (var i = Count - 1; i >= 0; i--)
			{
				var bu = this[i];
				if (businessRuleType == bu.GetType())
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

			foreach (var rule in ret)
			{
				rule.HaltModify = false;
			}

			return ret;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public static INewBusinessRuleCollection AllForScheduling(ISchedulingResultStateHolder schedulingResultStateHolder)
		{
			var ret = schedulingResultStateHolder.UseValidation
				? All(schedulingResultStateHolder)
				: MinimumAndPersonAccount(schedulingResultStateHolder, schedulingResultStateHolder.AllPersonAccounts);
			foreach (var rule in ret)
			{
				rule.HaltModify = false;
			}
			return ret;
		}

		public static INewBusinessRuleCollection MinimumAndPersonAccount(ISchedulingResultStateHolder schedulingResultStateHolder, IDictionary<IPerson, IPersonAccountCollection> personAccounts)
		{
			if (schedulingResultStateHolder == null) return null;
			var ret = new NewBusinessRuleCollection
			{
				new NewPersonAccountRule(schedulingResultStateHolder, personAccounts)
			};
			return ret;
		}

		public static INewBusinessRuleCollection NightlyRestRule()
		{
			var ret = new NewBusinessRuleCollection
			{
				new NewNightlyRestRule(new WorkTimeStartEndExtractor())
			};
			return ret;
		}

		public static INewBusinessRuleCollection New()
		{
			return new NewBusinessRuleCollection();
		}
	}
}