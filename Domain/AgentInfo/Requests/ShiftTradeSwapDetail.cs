using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo.Requests
{
    public class ShiftTradeSwapDetail : AggregateEntity, IShiftTradeSwapDetail
    {
        private readonly IPerson _personFrom;
        private readonly IPerson _personTo;
        private readonly DateOnly _dateFrom;
        private readonly DateOnly _dateTo;
        private long _checksumFrom;
        private long _checksumTo;

        protected ShiftTradeSwapDetail(){}

        public ShiftTradeSwapDetail(IPerson personFrom, IPerson personTo, DateOnly dateFrom, DateOnly dateTo) : this()
        {
            _personFrom = personFrom;
            _personTo = personTo;
            _dateFrom = dateFrom;
            _dateTo = dateTo;
        }

        public virtual DateOnly DateTo
        {
            get { return _dateTo; }
        }

        public virtual DateOnly DateFrom
        {
            get { return _dateFrom; }
        }

        public virtual IPerson PersonTo
        {
            get { return _personTo; }
        }

        public virtual IPerson PersonFrom
        {
            get { return _personFrom; }
        }

        public virtual long ChecksumFrom
        {
            get {
                return _checksumFrom;
            }
            set {
                _checksumFrom = value;
            }
        }

        public virtual long ChecksumTo
        {
            get {
                return _checksumTo;
            }
            set {
                _checksumTo = value;
            }
        }

        public virtual IScheduleDay SchedulePartFrom { get; set; }

        public virtual IScheduleDay SchedulePartTo { get; set; }
    }
}