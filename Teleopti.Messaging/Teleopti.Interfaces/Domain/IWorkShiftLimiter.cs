using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// An object of this types validates that a shift satisfies this limiter
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-18
    /// </remarks>
    public interface IWorkShiftLimiter : IAggregateEntity, ICloneableEntity<IWorkShiftLimiter>
    {
        /// <summary>
        /// Determines whether the projection is valid.
        /// Will be called when the work shift is fully created.
        /// </summary>
        /// <param name="endProjection">The end projection.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at end] [the specified end projection]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        bool IsValidAtEnd(IVisualLayerCollection endProjection);

        /// <summary>
        /// Determines whether the projection is valid.
        /// Will be called in the beginning of creating the workshift.
        /// Should not change the behaviour of IsValidAtEnd, only here
        /// to filter early because of performance reasons.
        /// </summary>
        /// <param name="shift">The template.</param>
        /// <param name="extenders">The extenders.</param>
        /// <returns>
        /// 	<c>true</c> if [is valid at start] [the specified template]; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-03-18
        /// </remarks>
        bool IsValidAtStart(IWorkShift shift, IList<IWorkShiftExtender> extenders);
    }
}