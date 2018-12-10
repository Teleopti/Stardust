using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;
using Teleopti.Ccc.UserTexts;

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
			var waitlistingIsEnabled = person.WorkflowControlSet.WaitlistingIsEnabled(absenceRequest);
			foreach (var affectedAccount in affectedAccounts)
			{
				if (affectedAccount.IsExceeded)
				{
					
					return error(waitlistingIsEnabled);
				}

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

				if (isAccountDay)
				{
					var numberDays = calculateDays(personRequest, scheduleDays);
					if (TimeSpan.FromDays(numberDays) > affectedAccount.Remaining)
					{
						return error(waitlistingIsEnabled);
					}

					affectedTimePerAccount = TimeSpan.FromDays(numberDays);
				}
				else if (isAccountTime)
				{
					var numberMinutes = calculateMinutes(personRequest, scheduleDays);
					if (TimeSpan.FromMinutes(numberMinutes).TotalMinutes > affectedAccount.Remaining.TotalMinutes)
					{
						return error(waitlistingIsEnabled);
					}

					affectedTimePerAccount = TimeSpan.FromMinutes(numberMinutes);
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

		private int calculateDays(IPersonRequest personRequest, List<IScheduleDay> scheduleDays)
		{
			int numberDays = 0;
			foreach (var day in scheduleDays)
			{
				if (absenseOnDay(day, personRequest))
					continue;
				var significantPart = day.SignificantPart();
				var isContractDayOff = _hasContractDayOffDefinition.IsDayOff(day);

				if (significantPart == SchedulePartView.MainShift || !isContractDayOff)
					numberDays++;
			}

			return numberDays;
		}

		private double calculateMinutes(IPersonRequest personRequest, List<IScheduleDay> scheduleDays)
		{
			double numberMinutes = 0;
			foreach (var day in scheduleDays)
			{
				var isContractDayOff = _hasContractDayOffDefinition.IsDayOff(day);
				if (day.HasDayOff() || day.IsFullDayAbsence() || isContractDayOff)
				{
					return numberMinutes;
				}

				var requestedPeriod = calculateRequestPeriod(personRequest, day);

				var visualLayerCollection = day.ProjectionService().CreateProjection();
				var visualLayerCollectionPeriod = visualLayerCollection.Period();
				if (day.IsScheduled() && visualLayerCollectionPeriod.HasValue)
				{
					numberMinutes += calculateMinutesBasedOnActualContractTime(personRequest, scheduleDays, requestedPeriod, visualLayerCollection);
				}
				else
				{
					numberMinutes += calculateMinutesBasedOnAverageContractTime(personRequest, requestedPeriod);
				}
			}

			return numberMinutes;
		}

		private static DateTimePeriod calculateRequestPeriod(IPersonRequest personRequest, IScheduleDay day)
		{
			var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();
			var scheduleDayDate = TimeZoneHelper.ConvertToUtc(day.DateOnlyAsPeriod.DateOnly.Date, timeZone);
			var requestedPeriod = personRequest.Request.Period;
			if (requestedPeriod.StartDateTime < scheduleDayDate)
			{
				requestedPeriod = new DateTimePeriod(scheduleDayDate, requestedPeriod.EndDateTime);
			}

			if (requestedPeriod.EndDateTime > scheduleDayDate.AddDays(1))
			{
				requestedPeriod = new DateTimePeriod(requestedPeriod.StartDateTime, scheduleDayDate.AddDays(1));
			}

			return requestedPeriod;
		}

		private static double calculateMinutesBasedOnActualContractTime(IPersonRequest personRequest, List<IScheduleDay> scheduleDays,  DateTimePeriod requestedPeriod, IVisualLayerCollection visualLayerCollection)
		{
			double numberMinutes;

			if (personRequest.Request is IAbsenceRequest absenceRequest && absenceRequest.FullDay)
			{
				numberMinutes = visualLayerCollection.ContractTime().TotalMinutes;
			}
			else
			{
				var contractTime = visualLayerCollection.FilterLayers(requestedPeriod).ContractTime();
				if (contractTime == TimeSpan.Zero)
				{
					contractTime = scheduleDays[0].ProjectionService().CreateProjection()
						.FilterLayers(requestedPeriod)
						.ContractTime();
				}

				numberMinutes = contractTime.TotalMinutes;
			}

			return numberMinutes;
		}

		private static double calculateMinutesBasedOnAverageContractTime(IPersonRequest personRequest, DateTimePeriod requestedPeriod)
		{
			var timeZone = personRequest.Person.PermissionInformation.DefaultTimeZone();

			var personPeriod = personRequest.Person.PersonPeriods(requestedPeriod.ToDateOnlyPeriod(timeZone))
									.FirstOrDefault();
			var personContract = personPeriod.PersonContract;
			var averageWorktimePerDayInMinutes =
				personContract.Contract.WorkTime.AvgWorkTimePerDay.TotalMinutes;
			var partTimePercentage = personContract.PartTimePercentage.Percentage.Value;
			var averageContractTimeSpan =
				TimeSpan.FromMinutes(averageWorktimePerDayInMinutes * partTimePercentage);

			var requestedTime = requestedPeriod.ElapsedTime() < averageContractTimeSpan
				? requestedPeriod.ElapsedTime()
				: averageContractTimeSpan;

			return requestedTime.TotalMinutes;
		}

		private static IValidatedRequest error(bool waitlistingIsenabled)
		{
			return waitlistingIsenabled 
				? new ValidatedRequest { IsValid = false, ValidationErrors = Resources.RequestWaitlistedReasonPersonAccount } 
				: new ValidatedRequest {IsValid = false, ValidationErrors = Resources.RequestDenyReasonPersonAccount};
		}
	}
}