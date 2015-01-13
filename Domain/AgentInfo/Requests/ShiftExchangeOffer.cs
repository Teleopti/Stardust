using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftExchangeOffer : Request, IShiftExchangeOffer
	{
		private readonly ShiftExchangeCriteria _criteria;
		private DateOnly _date;
		private DateTimePeriod? _myShiftPeriod;
		private IPerson _person;
		private long _checksum;
		private ShiftExchangeOfferStatus _status;
		private string _typeDescription;

		public ShiftExchangeOffer(IScheduleDay scheduleDay, ShiftExchangeCriteria criteria, ShiftExchangeOfferStatus status)
			: this()
		{
			_criteria = criteria;
			var projection = scheduleDay.ProjectionService().CreateProjection();
			_myShiftPeriod = projection.Period();
			_date = scheduleDay.DateOnlyAsPeriod.DateOnly;
			_person = scheduleDay.Person;
			_checksum = new ShiftTradeChecksumCalculator(scheduleDay).CalculateChecksum();
			_status = status;

			SetPeriod(criteria.ShiftWithin ?? scheduleDay.DateOnlyAsPeriod.Period());
		}

		protected ShiftExchangeOffer()
		{
			_typeDescription = Resources.Announcement;
		}

		public virtual DateOnly Date
		{
			get { return _date; }
		}

		public virtual DateTimePeriod? MyShiftPeriod
		{
			get { return _myShiftPeriod; }
		}

		protected internal override IEnumerable<IBusinessRuleResponse> Approve(IRequestApprovalService approvalService)
		{
			throw new NotImplementedException();
		}

		public override string RequestTypeDescription
		{
			get { return _typeDescription; }
			set { _typeDescription = value; }
		}

		public override RequestType RequestType
		{
			get { return RequestType.ShiftExchangeOffer; }
		}

		public override Description RequestPayloadDescription
		{
			get { return new Description(); }
		}

		public override void Deny(IPerson denyPerson)
		{
			TextForNotification = string.Format(UserTexts.Resources.AnnouncementInvalidMessage, _date.ToShortDateString());
		}

		public override void Accept(IPerson acceptingPerson, IShiftTradeRequestSetChecksum shiftTradeRequestSetChecksum,
			IPersonRequestCheckAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public override void Refer(IPersonRequestCheckAuthorization authorization)
		{
			throw new NotImplementedException();
		}

		public override string GetDetails(CultureInfo cultureInfo)
		{
			return string.Format(cultureInfo, "{0}", _date);
		}

		public virtual long Checksum
		{
			get { return _checksum; }
		}

		public virtual ShiftExchangeOfferStatus Status
		{
			get { return _status; }
			set { _status = value; }
		}

		public virtual string GetStatusText()
		{
			if (Status == ShiftExchangeOfferStatus.Pending)
			{
				return IsExpired() ? Resources.Expired : Resources.Pending;
			}
			if (Status == ShiftExchangeOfferStatus.Completed)
			{
				return Resources.Completed;
			}

			return Resources.Invalid;
		}		

		public virtual bool IsWantedSchedule(IScheduleDay scheduleToCheck)
		{
			var period = scheduleToCheck.ProjectionService().CreateProjection().Period();
			return _criteria.IsValid(period);
		}

		public virtual IPersonRequest MakeShiftTradeRequest(IScheduleDay scheduleToTrade)
		{
			return new PersonRequest(scheduleToTrade.Person, new ShiftTradeRequest(new IShiftTradeSwapDetail[]
			{
				new ShiftTradeSwapDetail(scheduleToTrade.Person, _person, _date, _date)
				{
					ChecksumFrom = new ShiftTradeChecksumCalculator(scheduleToTrade).CalculateChecksum(),
					ChecksumTo = _checksum
				}
			}) { Offer = this });
		}

		public virtual bool IsExpired()
		{
			return Date <= DateOnly.Today;
		}
	}
}