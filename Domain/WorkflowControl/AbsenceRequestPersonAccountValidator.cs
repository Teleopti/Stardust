using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AbsenceRequestPersonAccountValidator : IAbsenceRequestPersonAccountValidator
	{
		private readonly IPersonAbsenceAccountProvider _personAbsenceAccountProvider;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;

		public AbsenceRequestPersonAccountValidator(IPersonAbsenceAccountProvider personAbsenceAccountProvider, IHasContractDayOffDefinition hasContractDayOffDefinition)
		{
			_personAbsenceAccountProvider = personAbsenceAccountProvider;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
		}

		public IValidatedRequest Validate(IPersonRequest personRequest, IScheduleRange scheduleRange)
		{
			var person = personRequest.Person;
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			var workflowControlSet = person.WorkflowControlSet;
			
			var absenceRequestOpenPeriod = workflowControlSet.GetMergedAbsenceRequestOpenPeriod((IAbsenceRequest)personRequest.Request);
			if (absenceRequestOpenPeriod?.PersonAccountValidator is AbsenceRequestNoneValidator)
			{
				return ValidatedRequest.Valid;
			}

			var personAbsenceAccounts = _personAbsenceAccountProvider.Find(personRequest.Person);
			var personAbsenceAccount = personAbsenceAccounts.Find(absenceRequest.Absence);

			return ValidatedRequestWithPersonAccount(personRequest, scheduleRange, personAbsenceAccount);
		}

		public IValidatedRequest ValidatedRequestWithPersonAccount(IPersonRequest personRequest, IScheduleRange scheduleRange,
			IPersonAbsenceAccount personAbsenceAccount)
		{
			
			var person = personRequest.Person;

			var absenceRequest = personRequest.Request as IAbsenceRequest;
			if (personAbsenceAccount == null || personAbsenceAccount.AccountCollection().IsEmpty())
			{
				var calc = new EmptyPersonAccountBalanceCalculator(absenceRequest.Absence);
				var isOk = calc.CheckBalance(null, new DateOnlyPeriod());
				var validatedRequestNoAccount = new ValidatedRequest { IsValid = isOk };
				if (!isOk) validatedRequestNoAccount.DenyOption = PersonRequestDenyOption.InsufficientPersonAccount;
				return validatedRequestNoAccount;
			}

			var requestPeriod = personRequest.Request.Period.ToDateOnlyPeriod(person.PermissionInformation.DefaultTimeZone());

			var affectedAccounts = personAbsenceAccount.Find(requestPeriod).ToList();

			if (!affectedAccounts.Any())
				return new ValidatedRequest{IsValid = false};

			var validatedRequest = ValidatedRequest.Valid;

			foreach (var affectedAccount in affectedAccounts)
			{
				var waitlistingIsEnabled = person.WorkflowControlSet.WaitlistingIsEnabled(personRequest.Request as IAbsenceRequest);
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

				var affectedTimePerAccount = TimeSpan.Zero; 
				foreach (var day in scheduleDays)
				{
					if (isAccountDay)
					{
						if (absenseOnDay(day, personRequest))
							continue;

						numberDays = calculateDays(day, numberDays);
						if (TimeSpan.FromDays(numberDays) > affectedAccount.Remaining)
						{
							return error(waitlistingIsEnabled);
						}
						affectedTimePerAccount = TimeSpan.FromDays(numberDays);
					}
					else if (isAccountTime)
					{
						numberMinutes = calculateMinutes(personRequest, day, numberMinutes);
						if (TimeSpan.FromMinutes(numberMinutes).TotalMinutes > affectedAccount.Remaining.TotalMinutes)
						{
							return error(waitlistingIsEnabled);
						}
						affectedTimePerAccount = TimeSpan.FromMinutes(numberMinutes);
					}
				}
				validatedRequest.AffectedTimePerAccount.AddOrUpdate(affectedAccount, affectedTimePerAccount,
					(account, span) => affectedTimePerAccount);
			}

			return validatedRequest;
		}

		private static bool absenseOnDay(IScheduleDay day, IPersonRequest personRequest)
		{
			var absenceRequest = personRequest.Request as IAbsenceRequest;
			return day.PersonAbsenceCollection().Any(x => x.Layer.Payload == absenceRequest.Absence);
		}

		private int calculateDays(IScheduleDay day, int numberDays)
		{
			var significantPart = day.SignificantPart();
			var isContractDayOff = _hasContractDayOffDefinition.IsDayOff(day);
			
			if (significantPart == SchedulePartView.MainShift || !isContractDayOff)
				numberDays++;
			
			return numberDays;
		}

		private double calculateMinutes(IPersonRequest personRequest, IScheduleDay day, double numberMinutes)
		{
			var visualLayerCollection = day.ProjectionService().CreateProjection();
			var visualLayerCollectionPeriod = visualLayerCollection.Period();
			var isContractDayOff = _hasContractDayOffDefinition.IsDayOff(day);
			if (day.IsScheduled() && visualLayerCollectionPeriod.HasValue)
			{
				var contractTime = day.ProjectionService().CreateProjection()
					.FilterLayers(personRequest.Request.Period).ContractTime();
				numberMinutes += contractTime.TotalMinutes;
			}
			else if (!day.HasDayOff() && !day.IsFullDayAbsence() && !isContractDayOff)
			{
				var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
				var requestedPeriod = personRequest.Request.Period;
				var personPeriod = personRequest.Person.PersonPeriods(requestedPeriod.ToDateOnlyPeriod(timeZone)).FirstOrDefault();
				var personContract = personPeriod.PersonContract;
				var averageWorktimePerDayInMinutes = personContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes;
				var partTimePercentage = personContract.PartTimePercentage.Percentage.Value;
				var averageContractTimeSpan = TimeSpan.FromMinutes(averageWorktimePerDayInMinutes * partTimePercentage);

				var requestedTime = TimeSpan.Zero;
				requestedTime += requestedPeriod.ElapsedTime() < averageContractTimeSpan
					? requestedPeriod.ElapsedTime()
					: averageContractTimeSpan;
				numberMinutes += requestedTime.TotalMinutes;
			}
			
			return numberMinutes;
		}

		private static IValidatedRequest error(bool waitlistingIsenabled)
		{
			return waitlistingIsenabled 
				? new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestWaitlistedReasonPersonAccount } 
				: new ValidatedRequest {IsValid = false, ValidationErrors = Resources.RequestDenyReasonPersonAccount};
		}
	}
}