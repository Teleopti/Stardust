using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Scheduling.SchedulingScreenInternals
{
	public class ValidationRunner
	{
		private readonly ISchedulerStateHolder _schedulerStateHolder;

		public ValidationRunner(ISchedulerStateHolder schedulerStateHolder)
		{
			_schedulerStateHolder = schedulerStateHolder;
		}

		public void ValidatePersonAccountsOnly(IPerson person)
		{
			IScheduleRange range = _schedulerStateHolder.SchedulingResultState.Schedules[person];
			var rule = new NewPersonAccountRule(_schedulerStateHolder.SchedulingResultState.Schedules,
				_schedulerStateHolder.SchedulingResultState.AllPersonAccounts);
			IList<IBusinessRuleResponse> exposedBusinessRuleResponseCollection =
				((ScheduleRange)range).ExposedBusinessRuleResponseCollection();
			var toRemove = exposedBusinessRuleResponseCollection.Where(businessRuleResponse => businessRuleResponse.TypeOfRule == rule.GetType()).ToArray();
			foreach (var businessRuleResponse in toRemove)
			{
				exposedBusinessRuleResponseCollection.Remove(businessRuleResponse);
			}

			DateOnlyPeriod reqPeriod = _schedulerStateHolder.RequestedPeriod.DateOnlyPeriod;
			IEnumerable<IScheduleDay> allScheduleDays = range.ScheduledDayCollection(reqPeriod);
			var dic = new Dictionary<IPerson, IScheduleRange> { { person, range } };

			rule.Validate(dic, allScheduleDays);
		}

		public void ValidationOnLoad(BackgroundWorkerWrapper backgroundWorkerLoadData)
		{
			backgroundWorkerLoadData.ReportProgress(1,
				string.Format(CultureInfo.CurrentCulture, LanguageResourceHelper.Translate("XXValidatingPersons"),
					_schedulerStateHolder.ChoosenAgents.Count));
			var personsToValidate= new List<IPerson>();
			foreach (IPerson permittedPerson in _schedulerStateHolder.ChoosenAgents)
			{
				personsToValidate.Add(permittedPerson);
			}
			var rulesToRun = _schedulerStateHolder.SchedulingResultState.GetRulesToRun();

			var resolvedTranslatedString = LanguageResourceHelper.Translate("XXValidatingPersons2");
			var validatedCount = 0;
			foreach (var persons in personsToValidate.Batch(100))
			{
				var batchedPeople = persons as IList<IPerson> ?? persons.ToList();
				validatedCount += batchedPeople.Count;
				backgroundWorkerLoadData.ReportProgress(0,
					string.Format(CultureInfo.CurrentCulture, resolvedTranslatedString, validatedCount,
						_schedulerStateHolder.ChoosenAgents.Count));
				_schedulerStateHolder.Schedules.ValidateBusinessRulesOnPersons(batchedPeople, rulesToRun);
			}
		}
	}
}