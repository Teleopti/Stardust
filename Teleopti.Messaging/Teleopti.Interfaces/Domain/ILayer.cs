using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// 
	/// </summary>
	/// <typeparam name="T"></typeparam>
	public interface IPersistedLayer<T> : ILayer<T>, IAggregateEntity
	{}
    /// <summary>
    /// Base class for all layers
    /// </summary>
    /// <typeparam name="T">The type of the "payload"</typeparam>
    public interface ILayer<T> : ILayer, ICloneableEntity<ILayer<T>>, IPeriodized
    {
        /// <summary>
        /// Gets the name of the payload.
        /// </summary>
        /// <value>The name of the payload.</value>
        new T Payload { get; set; }
        
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        new DateTimePeriod Period { get; set; }

        /// <summary>
        /// Checks if to layers are adjacent.
        /// In other words - one layer starts when the other one ends.
        /// </summary>
        /// <param name="layer">The layer.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-30
        /// </remarks>
        bool AdjacentTo(ILayer<T> layer);

		/// <summary>
		/// Added because all layers not are IEntity any longer
		/// </summary>
		/// <param name="parent"></param>
		void SetParent(IEntity parent);

		/// <summary>
		/// The Parent
		/// </summary>
		IEntity Parent { get; }
	}

    /// <summary>
    /// Base interface for all layers
    /// Do not use this explicitly, use generic type instead
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-01-25
    /// </remarks>
    public interface ILayer 
    {
        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        DateTimePeriod Period { get; set; }

        /// <summary>
        /// Gets the name of the payload.
        /// </summary>
        /// <value>The name of the payload.</value>
        object Payload { get; set; }

        /// <summary>
        /// Changes a layer period end time with the time of a timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        void ChangeLayerPeriodEnd(TimeSpan timeSpan);

        /// <summary>
        /// Changes a layer period start time with the time of a timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        void ChangeLayerPeriodStart(TimeSpan timeSpan);

        /// <summary>
        /// Moves both start time and end time according to supplied timespan
        /// </summary>
        /// <param name="timeSpan"></param>
        void MoveLayer(TimeSpan timeSpan);

        /// <summary>
        /// Gets the index of the order.
        /// </summary>
        /// <value>The index of the order.</value>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-01-25
        /// </remarks>
        int OrderIndex { get; }

	    /// <summary>
	    /// 
	    /// </summary>
	    /// <param name="other"></param>
	    /// <returns></returns>
	    bool Equals(ILayer other);
    }
}
