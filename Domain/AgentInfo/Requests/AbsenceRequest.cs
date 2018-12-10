using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class AbsenceRequest : Request, IAbsenceRequest
	{
		private IAbsence _absence;
		private string _typeDescription = string.Empty;
		
		protected AbsenceRequest()
		{
			_typeDescription = UserTexts.Resources.RequestTypeAbsence;
		}

		public AbsenceRequest(IAbsence absence, DateTimePeriod period) : base(period)
		{
			_absence = absence;
			_typeDescription = Resources.RequestTypeAbsence;
		}
		
		public virtual IAbsence Absence => _absence;

		public virtual bool FullDay { get; set; }

		public virtual void SetAbsence(IAbsence absence)
		{
			if (Parent!= null && !((PersonRequest)Parent).IsEditable)
			{
				throw new InvalidOperationException("Requests cannot be changed once they have been handled.");
			}

			_absence = absence;
		}

		public override void Deny(IPerson denyPerson)
		{
			var hasBeenWaitlisted = ((PersonRequest)Parent).IsWaitlisted;

			setupTextForNotification(
				hasBeenWaitlisted ? Resources.AbsenceRequestForOneDayHasBeenWaitlisted : Resources.AbsenceRequestForOneDayHasBeenDeniedDot,
				hasBeenWaitlisted ? Resources.AbsenceRequestHasBeenWaitlisted : Resources.AbsenceRequestHasBeenDeniedDot);
		}

		public override void Cancel ()
		{
			setupTextForNotification(Resources.AbsenceRequestForOneDayWasCancelled, Resources.AbsenceRequestWasCancelled);
		}
		
		public virtual bool IsRequestForOneLocalDay(TimeZoneInfo timeZone)
		{
			return Period.StartDateTimeLocal(timeZone).Date == Period.EndDateTimeLocal(timeZone).Date;
		}

		public override string GetDetails(CultureInfo cultureInfo)
		{
			string text = Absence.Name;
			var timeZone = Person.PermissionInformation.DefaultTimeZone();
			var localStart = Period.StartDateTimeLocal(timeZone);
			var localEnd = Period.EndDateTimeLocal(timeZone);
			if (!localStart.AddDays(1).AddMinutes(-1).Equals(localEnd))
			{
				text = string.Format(cultureInfo, "{0}, {1} - {2}",
									 Absence.Name,
									 localStart.ToString("t",cultureInfo),
									 localEnd.ToString("t", cultureInfo));
			}
			return text;
		}

		protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
		{
			var personRequest = Parent as IPersonRequest;
			
			var result = approvalService.Approve(this);
			if (result.IsEmpty())
			{
				setupTextForNotification (Resources.AbsenceRequestForOneDayHasBeenApprovedDot, Resources.AbsenceRequestHasBeenApprovedDot);
				var approvedPersonAbsence = ((IAbsenceApprovalService)approvalService).GetApprovedPersonAbsence();
				approvedPersonAbsence?.IntradayAbsence(personRequest.Person,new TrackedCommandInfo
				{
					OperatedPersonId = personRequest.Person.Id.GetValueOrDefault(),
					TrackId = Guid.NewGuid()
				});
			}
			return result;
		}

		public override string RequestTypeDescription
		{
			get{ return _typeDescription;}
			set{_typeDescription = value;}
		}

		public override RequestType RequestType => RequestType.AbsenceRequest;

		public override Description RequestPayloadDescription => _absence.Description;

		private void setupTextForNotification(string oneDayRequestMessage, string requestMessage)
		{
			var timeZone = Person.PermissionInformation.DefaultTimeZone();
			var culture = Person.PermissionInformation.Culture();

			if (IsRequestForOneLocalDay(timeZone))
			{
				TextForNotification =
					string.Format(culture, oneDayRequestMessage,
						Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
			}
			else
			{
				TextForNotification =
					string.Format(culture, requestMessage,
						Period.StartDateTimeLocal(timeZone).Date.ToString(
							culture.DateTimeFormat.ShortDatePattern, culture),
						Period.EndDateTimeLocal(timeZone).Date.ToString(
							culture.DateTimeFormat.ShortDatePattern, culture));
			}
		}
	}
}