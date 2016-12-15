using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Rules
{
	public class NewPersonAccountRule : INewBusinessRule
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;

		//private readonly ISchedulingResultStateHolder _schedules;
		private readonly IDictionary<IPerson, IPersonAccountCollection> _allAccounts;

		private bool _haltModify = true;
		private bool _forDelete;
		private static readonly object modifiedAccountLock = new object();
		private readonly string _businessRulePersonAccountError1;

		public NewPersonAccountRule(ISchedulingResultStateHolder schedulingResultStateHolder, IDictionary<IPerson, IPersonAccountCollection> allAccounts)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_allAccounts = allAccounts;
			//_schedules = schedules;
			FriendlyName = Resources.BusinessRulePersonAccountFriendlyName;
			Description = Resources.DescriptionOfNewPersonAccountRule;
			_businessRulePersonAccountError1 = Resources.BusinessRulePersonAccountError1;
		}

		public bool IsMandatory
		{
			get { return false; }
		}

		public bool HaltModify
		{
			get { return _haltModify; }
			set { _haltModify = value; }
		}

		public bool Configurable => false;

		public bool ForDelete
		{
			get { return _forDelete; }
			set { _forDelete = value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
		public IEnumerable<IBusinessRuleResponse> Validate(IDictionary<IPerson, IScheduleRange> rangeClones, IEnumerable<IScheduleDay> scheduleDays)
		{
			var responseList = new List<IBusinessRuleResponse>();

			var firstOrDefault = rangeClones.Values.FirstOrDefault();
			if (firstOrDefault == null || !firstOrDefault.Scenario.DefaultScenario)
			{
				return responseList;
			}

			var enumerable = scheduleDays.ToLookup(s => s.Person);
			foreach (var rangeCloneValueKey in rangeClones)
			{
				IPersonAccountCollection myAccounts;
				if (!_allAccounts.TryGetValue(rangeCloneValueKey.Key, out myAccounts))
					continue;

				var affectedAccounts = new HashSet<IAccount>();
				foreach (var scheduleDay in enumerable[rangeCloneValueKey.Key])
				{
					DateOnly dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
					IEnumerable<IAccount> accounts = myAccounts.Find(dateOnly);
					foreach (var account in accounts)
					{
						affectedAccounts.Add(account);
					}
				}

				var timeZone = rangeCloneValueKey.Value.Person.PermissionInformation.DefaultTimeZone();
				foreach (var affectedAccount in affectedAccounts)
				{
					DateOnlyPeriod rangePeriod = rangeCloneValueKey.Value.Period.ToDateOnlyPeriod(timeZone);
					DateOnlyPeriod? intersection = affectedAccount.Period().Intersection(rangePeriod);
					if (!intersection.HasValue)
						continue;

					var lastRemaining = affectedAccount.Remaining;
					IList<IScheduleDay> scheduleDaysForAccount = new List<IScheduleDay>(rangeCloneValueKey.Value.ScheduledDayCollection(intersection.Value));
					affectedAccount.Owner.Absence.Tracker.Track(affectedAccount, affectedAccount.Owner.Absence,
																			  scheduleDaysForAccount);

					if (lastRemaining != affectedAccount.Remaining)
					{
						if (_schedulingResultStateHolder.Schedules == null)
							_schedulingResultStateHolder.Schedules = rangeClones.Values.FirstOrDefault().Owner;
						//Tell someone this account is dirty
						lock (modifiedAccountLock) { }
							_schedulingResultStateHolder.Schedules.ModifiedPersonAccounts.Add((IPersonAbsenceAccount) affectedAccount.Root());
					}

					var oldResponses = rangeCloneValueKey.Value.BusinessRuleResponseInternalCollection;
					foreach (var scheduleDay in scheduleDaysForAccount)
					{
						oldResponses.Remove(createResponse(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod, string.Empty, typeof(NewPersonAccountRule), affectedAccount.Owner.Absence.Id));
					}
					if(affectedAccount.IsExceeded)
					{
						foreach (var scheduleDay in scheduleDaysForAccount)
						{
							string message = string.Format(TeleoptiPrincipal.CurrentPrincipal.Regional.Culture,
													_businessRulePersonAccountError1,
													affectedAccount.Owner.Absence.Description.Name, affectedAccount.Period().StartDate.ToShortDateString());

							if (!ForDelete)
							{
								var response = createResponse(scheduleDay.Person, scheduleDay.DateOnlyAsPeriod, message,
									typeof(NewPersonAccountRule), affectedAccount.Owner.Absence.Id);
								responseList.Add(response);
							}
						}
					}
				}
				foreach (var businessRuleResponse in responseList)
				{
					if (rangeCloneValueKey.Key.Equals(businessRuleResponse.Person))
						rangeCloneValueKey.Value.BusinessRuleResponseInternalCollection.Add(businessRuleResponse);
				}
			}

			return responseList;
		}

		public string FriendlyName { get; }
		public string Description { get; }

		private IBusinessRuleResponse createResponse(IPerson person, IDateOnlyAsDateTimePeriod dateOnly, string message, Type type, Guid? absenceId)
		{
			var dop = dateOnly.DateOnly.ToDateOnlyPeriod();
			IBusinessRuleResponse response = new BusinessRuleResponseWithAbsenceId(type, message, _haltModify, IsMandatory, dateOnly.Period(), person, dop, FriendlyName, absenceId) { Overridden = !_haltModify };
			return response;
		}
	}

	public class BusinessRuleResponseWithAbsenceId : BusinessRuleResponse
	{
		public Guid? AbsenceId { get; set; }

		public BusinessRuleResponseWithAbsenceId(Type typeOfRule, string message, bool error, bool mandatory, DateTimePeriod period, IPerson person, DateOnlyPeriod dateOnlyPeriod, string friendlyName, Guid? absenceId) : base(typeOfRule, message, error, mandatory, period, person, dateOnlyPeriod, friendlyName)
		{
			AbsenceId = absenceId;
		}
	}
}