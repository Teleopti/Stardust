﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    /// <summary>
    /// Contains a new text request
    /// </summary>
    public class TextRequest : Request
    {
        private string _typeDescription = UserTexts.Resources.RequestTypeText;
        private const string _textRequestHasBeenApprovedDot = "TextRequestHasBeenApprovedDot";
        private const string _textRequestHasBeenDeniedDot = "TextRequestHasBeenDeniedDot";

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

	    public override void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum, IPersonRequestCheckAuthorization authorization)
        {
            throw new NotImplementedException();
        }

        public override void Refer(IPersonRequestCheckAuthorization authorization)
        {
            throw new NotImplementedException();
        }

        public override string GetDetails(CultureInfo cultureInfo)
        {
            string text = string.Format(cultureInfo, "{0}, {1} - {2}",
                                        UserTexts.Resources.TextRequest,
                                        Period.LocalStartDateTime.ToString("t", cultureInfo),
                                        Period.LocalEndDateTime.ToString("t", cultureInfo));
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
