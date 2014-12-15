using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
	public class ShiftExchangeOffer : NonversionedAggregateRootWithBusinessUnit, IShiftExchangeOffer
	{
		private readonly ShiftExchangeCriteria _criteria;
		private DateOnly _date;
		private DateTimePeriod? _myShiftPeriod;
		private IPerson _person;
		private long _checksum;
		private ShiftExchangeOfferStatus _status;

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
		}

		protected ShiftExchangeOffer()
		{
		}

		public virtual DateOnly Date
		{
			get { return _date; }
		}

		public virtual DateTimePeriod? MyShiftPeriod
		{
			get { return _myShiftPeriod; }
		}

		public virtual IPerson Person
		{
			get { return _person; }
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
	}
}