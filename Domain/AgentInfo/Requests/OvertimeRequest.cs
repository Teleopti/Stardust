using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequest : Request, IOvertimeRequest
	{
		private readonly IMultiplicatorDefinitionSet _multiplicatorDefinitionSet;
		private string _typeDescription;
		
		protected OvertimeRequest()
		{
			_typeDescription = Resources.RequestTypeOvertime;
		}

		public OvertimeRequest(IMultiplicatorDefinitionSet multiplicatorDefinitionSet, DateTimePeriod period) : base(period)
		{
			_multiplicatorDefinitionSet = multiplicatorDefinitionSet;
			_typeDescription = Resources.RequestTypeOvertime;
		}

		public virtual IMultiplicatorDefinitionSet MultiplicatorDefinitionSet => _multiplicatorDefinitionSet;

		public override void Deny(IPerson denyPerson)
		{
			var timeZone = Person.PermissionInformation.DefaultTimeZone();
			var culture = Person.PermissionInformation.Culture();
			TextForNotification =
					string.Format(culture, Resources.OvertimeRequestForOneDayHasBeenDeniedDot,
						Period.StartDateTimeLocal(timeZone).Date.ToString("d", culture));
		}

		public override void Cancel ()
		{
			TextForNotification = Resources.OvertimeRequestWasCancelled;
		}

		public override string GetDetails(CultureInfo cultureInfo)
		{
			var text = _multiplicatorDefinitionSet.Name;
			var timeZone = Person.PermissionInformation.DefaultTimeZone();
			var localStart = Period.StartDateTimeLocal(timeZone);
			var localEnd = Period.EndDateTimeLocal(timeZone);
			if (!localStart.AddDays(1).AddSeconds(-1).Equals(localEnd))
			{
				text = string.Format(cultureInfo, "{0}, {1} - {2}",
									 _multiplicatorDefinitionSet.Name,
									 localStart.ToString("t", cultureInfo),
									 localEnd.ToString("t", cultureInfo));
			}
			return text;
		}

		protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
		{
			var result = approvalService.Approve(this);
			if (result.IsEmpty())
			{
				var timeZone = Person.PermissionInformation.DefaultTimeZone();
				var culture = Person.PermissionInformation.Culture();
				TextForNotification = string.Format(culture, Resources.OvertimeRequestHasBeenApprovedDot,
					Period.StartDateTimeLocal(timeZone).ToString(culture.DateTimeFormat.FullDateTimePattern, culture),
					Period.EndDateTimeLocal(timeZone).ToString(culture.DateTimeFormat.FullDateTimePattern, culture));
			}
			return result;
		}

		public override string RequestTypeDescription
		{
			get{ return _typeDescription;}
			set{_typeDescription = value;}
		}

		public override RequestType RequestType => RequestType.OvertimeRequest;

		public override Description RequestPayloadDescription => new Description(_multiplicatorDefinitionSet.Name);
	}
}