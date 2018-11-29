using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    /// <summary>
    /// Contains a new text request
    /// </summary>
    public class TextRequest : Request
    {
        private string _typeDescription = UserTexts.Resources.RequestTypeText;
        private const string _textRequestHasBeenApprovedDot =  nameof(Resources.TextRequestHasBeenApprovedDot);
        private const string _textRequestHasBeenDeniedDot = nameof(Resources.TextRequestHasBeenDeniedDot);

        /// <summary>
        /// Constructor for NHibernate
        /// </summary>
        protected TextRequest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextRequest"/> class.
        /// </summary>
        /// <param name="period">The period.</param>
        public TextRequest(DateTimePeriod period)
            : base(period)
        {
        }

        /// <summary>
        /// Denies this instance.
        /// </summary>
        public override void Deny(IPerson denyPerson)
        {
            TextForNotification = _textRequestHasBeenDeniedDot;
        }

	    public override void Cancel()
	    {
		    throw new NotImplementedException();
	    }

        public override string GetDetails(CultureInfo cultureInfo)
        {
	        var timeZone = Person.PermissionInformation.DefaultTimeZone();
            string text = string.Format(cultureInfo, "{0}, {1} - {2}",
                                        UserTexts.Resources.TextRequest,
                                        Period.StartDateTimeLocal(timeZone).ToString("t", cultureInfo),
                                        Period.EndDateTimeLocal(timeZone).ToString("t", cultureInfo));
            return text;
        }

        protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
        {
            TextForNotification = _textRequestHasBeenApprovedDot;
            return new List<IBusinessRuleResponse>();
        }

        /// <summary>
        /// Description for type of request
        /// </summary>
        /// <value></value>
        public override string RequestTypeDescription
        {
            get
            {
                return _typeDescription;
            }
            set
            {
                _typeDescription = value;
            }
        }

    	public override RequestType RequestType
    	{
    		get { return RequestType.TextRequest; }
    	}

    	/// <summary>
        /// Payload description
        /// </summary>
        public override Description RequestPayloadDescription
        {
            get { return new Description(); }
        }
    }

}
