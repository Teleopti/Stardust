using System;
using System.Drawing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class DayOff
    {
	    private DateTime _anchor;
        private TimeSpan _targetLength;
        private TimeSpan _flexibility;
        private readonly Description _description;
        private readonly Color _displayColor;
		private readonly string _payrollCode;

		protected DayOff()
		{
		}

		public DayOff(DateTime anchor, TimeSpan targetLength, TimeSpan flexibility, Description description, Color displayColor, string payrollCode, Guid dayOffTemplateId)
		{
			InParameter.VerifyDateIsUtc(nameof(anchor), anchor);

			_anchor = anchor;
			_targetLength = targetLength;
			if (flexibility.TotalMinutes > targetLength.TotalMinutes / 2d)
				flexibility = TimeSpan.FromMinutes(targetLength.TotalMinutes / 2);
			_flexibility = flexibility;
			_description = description;
			_displayColor = displayColor;
			_payrollCode = payrollCode;
			DayOffTemplateId = dayOffTemplateId;
		}

		public TimeSpan Flexibility => _flexibility;

        public TimeSpan TargetLength => _targetLength;

        public DateTime Anchor => _anchor;

        public Description Description => _description;

        public DateTime AnchorLocal(TimeZoneInfo targetTimeZone)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(_anchor, targetTimeZone);
         }

        public Color DisplayColor => _displayColor;
		
	    public Guid DayOffTemplateId { get; }

        public DateTimePeriod Boundary
        {
            get
            {
                double minutes = _targetLength.TotalMinutes / 2d + (_flexibility.TotalMinutes);

                return new DateTimePeriod(
                    _anchor.AddMinutes(-minutes),
                    _anchor.AddMinutes(minutes));
            }
        }

        public DateTimePeriod InnerBoundary
        {
            get
            {
               return new DateTimePeriod(
                    _anchor.AddMinutes(-_targetLength.TotalMinutes / 2d).AddMinutes(_flexibility.TotalMinutes),
                    _anchor.AddMinutes(_targetLength.TotalMinutes / 2d).AddMinutes(-_flexibility.TotalMinutes));
            }
        }
		
    	public string PayrollCode => _payrollCode;

	    public override bool Equals(object obj)
        {
	        var other = obj as DayOff;
					if (other == null)
					{
						return false;
					}
	        return other._anchor == _anchor &&
	               other._targetLength == _targetLength &&
	               other._flexibility == _flexibility &&
	               other._payrollCode == _payrollCode;
        }

        public override int GetHashCode()
        {
			return _anchor.GetHashCode() ^ _targetLength.GetHashCode() ^ _flexibility.GetHashCode();
        }
    }
}