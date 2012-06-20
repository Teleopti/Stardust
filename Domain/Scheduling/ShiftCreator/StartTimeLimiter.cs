using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    /// <summary>
    /// Limiter for start time
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2008-11-17
    /// </remarks>
    public class StartTimeLimiter : WorkShiftLimiter
    {
        private TimePeriod _startTimeLimitation;

        /// <summary>
        /// Initializes a new instance of the <see cref="StartTimeLimiter"/> class.
        /// </summary>
        /// <param name="startTimeLimitation">The start time limitation.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public StartTimeLimiter(TimePeriod startTimeLimitation)
        {
            _startTimeLimitation = startTimeLimitation;
        }

        /// <summary>
        /// Gets or sets the start time limitation.
        /// </summary>
        /// <value>The start time limitation.</value>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2008-11-17
        /// </remarks>
        public TimePeriod StartTimeLimitation
        {
            get { return _startTimeLimitation; }
            set { _startTimeLimitation = value; }
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
            StartTimeLimiter retobj = (StartTimeLimiter)MemberwiseClone();
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
            return (StartTimeLimiter)MemberwiseClone();
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
        	return endProjection!=null && endProjection.Any();
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
            if (_startTimeLimitation.ContainsPart(shift.LayerCollection.Period().Value.TimePeriod(TeleoptiPrincipal.Current.Regional.TimeZone).StartTime))
            {
                return true;
            }

            return false;
        }
    }
}
