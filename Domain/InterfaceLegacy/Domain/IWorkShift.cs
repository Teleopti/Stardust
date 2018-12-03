using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Interface for work shifts
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-03-03
    /// </remarks>
	public interface IWorkShift : ILayerCollectionOwner<IActivity>, ICloneableEntity<IWorkShift>, IProjectionSource
    {
        /// <summary>
        /// Gets the shift category.
        /// </summary>
        /// <value>The shift category.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-04-10
        /// </remarks>
        IShiftCategory ShiftCategory { get; }

        /// <summary>
        /// Gets the projection.
        /// </summary>
        /// <value>The projection.</value>
        /// <remarks>
        /// Created by: henrika
        /// Created date: 2008-05-09
        /// </remarks>
        IVisualLayerCollection Projection { get; }

	    /// <summary>
        /// Convert this instance to a main shift
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2009-03-02
        /// </remarks>
		IEditableShift ToEditorShift(IDateOnlyAsDateTimePeriod dateOnlyAsDateTimePeriod, TimeZoneInfo localTimeZone);

        /// <summary>
        /// Convert the workshift datetime period to timeperiod.
        /// </summary>
        /// <returns></returns>
        TimePeriod? ToTimePeriod();
    }
}