using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestPersonAccountValidator : IAbsenceRequestPersonAccountValidator
	{
		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;

		public AbsenceRequestPersonAccountValidator(IPersonAbsenceAccountProvider personAbsenceAccountProvider)
		{
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
		}

		public IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange)
		{
			var person = personRequest.Person;
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			var workflowControlSet = person.WorkflowControlSet;
			
			var absenceRequestOpenPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
			var waitlistingIsEnabled = workflowControlSet.WaitlistingIsEnabled (absenceRequest);


			if (absenceRequestOpenPeriod?.PersonAccountValidator is AbsenceRequestNoneValidator)
			{
				return ValidatedRequest.Valid;
			}

			var personAbsenceAccounts = _personAbsenceAccountProvider.Find(personRequest.Person);
			var personAbsenceAccount = personAbsenceAccounts.Find(absenceRequest.Absence);
			var requestPeriod = personRequest.Request.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());
			var affectedAccounts = personAbsenceAccount?.Find(requestPeriod);

			if (affectedAccounts == null)
				return ValidatedRequest.Valid;

			foreach (var affectedAccount in affectedAccounts)
			{
				if (affectedAccount.IsExceeded)
				{
					return error(waitlistingIsEnabled);
				}

				var numberDays = 0;
				var numberMinutes = 0d;
				var isAccountDay = affectedAccount is AccountDay;
				var isAccountTime = affectedAccount is AccountTime;

				var intersectingPeriod = affectedAccount.Period().Intersection(requestPeriod);
				if (!intersectingPeriod.HasValue)
				{
					continue;
				}

				var scheduleDays =
					new List<IScheduleDay>(scheduleRange.ScheduledDayCollection(intersectingPeriod.Value));

				foreach (var day in scheduleDays)
				{
					if (isAccountDay)
					{
						numberDays = calculateDays(day, numberDays);
						if (TimeSpan.FromDays(numberDays) > affectedAccount.Remaining)
						{
							return error(waitlistingIsEnabled);
						}
					}
					else if (isAccountTime)
					{
						numberMinutes = calculateMinutes(personRequest, day, numberMinutes);
						if (TimeSpan.FromMinutes(numberMinutes).TotalMinutes > affectedAccount.Remaining.TotalMinutes)
						{
							return error(waitlistingIsEnabled);
						}
					}
				}
			}

			return ValidatedRequest.Valid;
		}

		private static int calculateDays(IScheduleDay day, int numberDays)
		{
			var significantPart = day.SignificantPart();
			if (significantPart != SchedulePartView.DayOff && significantPart != SchedulePartView.ContractDayOff)
				numberDays++;
			return numberDays;
		}

		private static double calculateMinutes(IPersonRequest personRequest, IScheduleDay day, double numberMinutes)
		{
			var contractTime = day.ProjectionService().CreateProjection()
				.FilterLayers(personRequest.Request.Period).ContractTime();
			numberMinutes += contractTime.TotalMinutes;
			return numberMinutes;
		}

		private IValidatedRequest error(bool waitlistingIsenabled)
		{
			return waitlistingIsenabled 
				? new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestWaitlistedReasonPersonAccount } 
				: new ValidatedRequest {IsValid = false, ValidationErrors = Resources.RequestDenyReasonPersonAccount};
		}
	}
}