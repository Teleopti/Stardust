﻿using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class OvertimeRequest : Request, IOvertimeRequest
	{
		private readonly IActivity _activity;
		private readonly OvertimeType _overtimeType;
		private string _typeDescription;
		
		protected OvertimeRequest()
		{
			_typeDescription = Resources.RequestTypeOvertime;
		}

		public OvertimeRequest(IActivity activity, OvertimeType overtimeType, DateTimePeriod period) : base(period)
		{
			_activity = activity;
			_overtimeType = overtimeType;
			_typeDescription = Resources.RequestTypeOvertime;
		}

	 
		public virtual IActivity Activity => _activity;

		public virtual OvertimeType OvertimeType => _overtimeType;

		public override void Deny(IPerson denyPerson)
		{
			throw new NotImplementedException();
		}

		public override void Cancel ()
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
			throw new NotImplementedException();
		}

		protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
		{
			throw new NotImplementedException();
		}

		public override string RequestTypeDescription
		{
			get{ return _typeDescription;}
			set{_typeDescription = value;}
		}

		public override RequestType RequestType => RequestType.OvertimeRequest;

		public override Description RequestPayloadDescription => _activity.Description;
	}
}