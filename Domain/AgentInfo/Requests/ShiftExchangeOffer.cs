using System;
using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftExchangeOffer : Request, IShiftExchangeOffer
	{
		private IShiftExchangeCriteria _criteria;
		private DateOnly _date;
		private DateTimePeriod? _myShiftPeriod;
		private IPerson _person;
		private long _checksum;
		private ShiftExchangeOfferStatus _status;
		private string _typeDescription;
		private string _shiftExchangeOfferId;

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
			set { _date = value; }
		}

		public virtual IShiftExchangeCriteria Criteria
		{
			get { return _criteria; }
			set { _criteria = value; }
		}

		public virtual DateOnly ValidTo
		{
			get { return _criteria.ValidTo; }
			set { _criteria.ValidTo = value; }
		}

		public virtual ShiftExchangeLookingForDay DayType
		{
			get { return _criteria.DayType; }
			set { _criteria.DayType = value; }
		}

		public virtual DateTimePeriod? MyShiftPeriod
		{
			get { return _myShiftPeriod; }
			set { _myShiftPeriod = value; }
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

		public override RequestType RequestType => RequestType.ShiftExchangeOffer;

		public override Description RequestPayloadDescription => new Description();

		public override void Deny(IPerson denyPerson)
		{
			TextForNotification = string.Format(Resources.AnnouncementInvalidMessage, _date.ToShortDateString());
		}

		public override void Cancel()
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
			set { _checksum = value; }
		}

		public virtual string ShiftExchangeOfferId
		{
			get { return _shiftExchangeOfferId; }
			set { _shiftExchangeOfferId = value; }
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
			var period = scheduleToCheck?.ProjectionService().CreateProjection().Period();		
			return _criteria.IsValid(period.HasValue ? (DateTimePeriod?)period.Value : null, scheduleToCheck != null && scheduleToCheck.HasDayOff());
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
			return Date <= new DateOnly(TimeZoneHelper.ConvertFromUtc(ServiceLocatorForEntity.Now.UtcDateTime(),
					   _person.PermissionInformation.DefaultTimeZone()));
		}				
	}
}