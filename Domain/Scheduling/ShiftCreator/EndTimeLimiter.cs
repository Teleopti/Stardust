using System.Collections.Generic;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Limiter for en time
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-11-17
    /// </remarks>
    public class EndTimeLimiter : WorkShiftLimiter
    {
        private TimePeriod _endTimeLimitation;

        /// <summary>
        /// Initializes a new instance of the <see cref="EndTimeLimiter"/> class.
        /// </summary>
        /// <param name="endTimeLimitation">The end time limitation.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public EndTimeLimiter(TimePeriod endTimeLimitation)
        {
            _endTimeLimitation = endTimeLimitation;
        }

        /// <summary>
        /// Gets or sets the end time limitation.
        /// </summary>
        /// <value>The end time limitation.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public TimePeriod EndTimeLimitation
        {
            get { return _endTimeLimitation; }
            set { _endTimeLimitation = value; }
        }

        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public override object Clone()
        {
            return EntityClone();
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id set to null.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public override IWorkShiftLimiter NoneEntityClone()
        {
            EndTimeLimiter retobj = (EndTimeLimiter)MemberwiseClone();
            retobj.SetId(null);
            retobj.SetParent(null);
            return retobj;
        }

        /// <summary>
        /// Returns a clone of this T with IEntitiy.Id as this T.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public override IWorkShiftLimiter EntityClone()
        {
            return (EndTimeLimiter)MemberwiseClone();
        }

        /// <summary>
        /// Determines whether there are shifts left.
        /// </summary>
        /// <param name="endProjection">The end projection.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at end] [the specified end projection]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public override bool IsValidAtEnd(IVisualLayerCollection endProjection)
        {
            //Shifts remaining after IsValidAtStart should always be true?
            if (endProjection.Count() > 0)
                return true;
            return false;
        }

        /// <summary>
        /// Determines whether a shift is valid due to this limiter.
        /// </summary>
        /// <param name="shift">The shift.</param>
        /// <param name="extenders">The extenders.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at start] [the specified shift]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public override bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders)
        {
            //Check what kind of time shoud be used UTC or local
            if (_endTimeLimitation.ContainsPart(shift.LayerCollection.Period().Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).EndTime))
            {
                return true;
            }

            return false;
        }
    }
}
