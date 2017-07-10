using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    /// <summary>
    /// Contains a new request for absence
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2008-06-05
    /// </remarks>
    public class AbsenceRequest : Request, IAbsenceRequest
    {
        private readonly IAbsence _absence;
        private string _typeDescription = string.Empty;
	    
	    /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequest"/> class.
        /// For NHibernate to use.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        protected AbsenceRequest()
        {
            _typeDescription = UserTexts.Resources.RequestTypeAbsence;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AbsenceRequest"/> class.
        /// </summary>
        /// <param name="absence">The absence.</param>
        /// <param name="period">The period.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
        public AbsenceRequest(IAbsence absence, DateTimePeriod period) : base(period)
        {
            _absence = absence;
            _typeDescription = Resources.RequestTypeAbsence;
        }

     
        public virtual IAbsence Absence
        {
            get { return _absence; }
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
	        if (!localStart.AddDays(1).AddSeconds(-1).Equals(localEnd))
            {
                text = string.Format(cultureInfo, "{0}, {1} - {2}",
                                     Absence.Name,
                                     localStart.ToString("t",cultureInfo),
                                     localEnd.ToString("t", cultureInfo));
            }
            return text;
        }

        /// <summary>
        /// Approves this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-06-05
        /// </remarks>
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

        /// <summary>
        /// Description for the request type
        /// </summary>
        public override string RequestTypeDescription
        {
            get{ return _typeDescription;}
            set{_typeDescription = value;}
        }

    	public override RequestType RequestType
    	{
			get { return RequestType.AbsenceRequest; }
    	}

    	/// <summary>
        /// Description for the absence
        /// </summary>
        public override Description RequestPayloadDescription
        {
            get{return _absence.Description;}
        }

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