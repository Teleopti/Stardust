using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    /// <summary>
    /// Layer class containing Absences
    /// </summary>
    public class AbsenceLayer : Layer<IAbsence>, IAbsenceLayer
    {
        /// <summary>
        /// Used by nhibernate to reconstitute from datasource
        /// </summary>
        protected AbsenceLayer()
        {
        }

        /// <summary>
        ///  Initializes a new instance of the AbsenceLayer class
        /// </summary>
        /// <param name="abs"></param>
        /// <param name="period"></param>
        public AbsenceLayer(IAbsence abs, DateTimePeriod period) : base(abs, period)
        {
            InParameter.EnsureNoSecondsInPeriod(period);
        }

        public override DateTimePeriod Period
        {
            get
            {
                return base.Period;
            }
            set
            {
                InParameter.EnsureNoSecondsInPeriod(value);
                base.Period = value;
            }
        }

        public override void ChangeLayerPeriodEnd(TimeSpan timeSpan)
        {
            InParameter.EnsureNoSecondsInTimeSpan(timeSpan);
            base.ChangeLayerPeriodEnd(timeSpan);
        }

        public override void ChangeLayerPeriodStart(TimeSpan timeSpan)
        {
            InParameter.EnsureNoSecondsInTimeSpan(timeSpan);
            base.ChangeLayerPeriodStart(timeSpan);
        }

        public override void MoveLayer(TimeSpan timeSpan)
        {
            InParameter.EnsureNoSecondsInTimeSpan(timeSpan);
            base.MoveLayer(timeSpan);
        }
    }
}