using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class AbsenceLayer : Layer<IAbsence>, IAbsenceLayer
	{
		protected AbsenceLayer()
		{
		}

		public AbsenceLayer(IAbsence abs, DateTimePeriod period) : base(abs, period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
		}

		public override DateTimePeriod Period
		{
			get { return base.Period; }
			set
			{
				InParameter.EnsureNoSecondsInPeriod(value);
				base.Period = value;
			}
		}
	}
}