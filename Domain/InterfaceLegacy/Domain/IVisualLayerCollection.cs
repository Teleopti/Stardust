using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Interface for visuallayercollection
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-03-06
	/// </remarks>
	public interface IVisualLayerCollection : IEnumerable<IVisualLayer>
	{
		/// <summary>
		/// Gets the total contract time for this collection.
		/// Remember that this might not be the correct result
		/// if the projection is built by a part of the total
		/// schedule, eg a personassignment.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-27
		/// </remarks>
		TimeSpan ContractTime();

		/// <summary>
		/// Gets the total work time.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-11-25
		/// </remarks>
		TimeSpan WorkTime();

		/// <summary>
		/// Gets the total paid time.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-11-25
		/// </remarks>
		TimeSpan PaidTime();

		/// <summary>
		/// Gets the totoal Over time.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2009-02-24
		/// </remarks>
		TimeSpan Overtime();

		/// <summary>
		/// Gets the period of the hole collection.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-02-26
		/// </remarks>
		DateTimePeriod? Period();

		/// <summary>
		/// Filters the layers per activity.
		/// </summary>
		/// <param name="payloadToSearch">The activity to search.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-03-04
		/// </remarks>
		IFilteredVisualLayerCollection FilterLayers(IPayload payloadToSearch);

		/// <summary>
		/// Filters the layers per period.
		/// </summary>
		/// <param name="periodToSearch">The period to search.</param>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2008-04-18
		/// </remarks>
		IFilteredVisualLayerCollection FilterLayers(DateTimePeriod periodToSearch);
		
		/// <summary>
		/// Has this collection layers?
		/// </summary>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-04-18
		/// </remarks>
		bool HasLayers { get; }

		/// <summary>
		/// Gets the count.
		/// RK: If you just want to know it empty; use HasLayers instead - much faster!
		/// </summary>
		/// <value>The count.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-11-05
		/// </remarks>
		int Count();

		/// <summary>
		/// Determines whether [is satisfied by] [the specified specification].
		/// </summary>
		/// <param name="specification">The specification.</param>
		/// <returns>
		/// 	<c>true</c> if [is satisfied by] [the specified specification]; otherwise, <c>false</c>.
		/// </returns>
		/// <remarks>
		/// Created by: rogerkr
		/// Created date: 2008-10-22
		/// </remarks>
		bool IsSatisfiedBy(ISpecification<IVisualLayerCollection> specification);

		/// <summary>
		/// Filters the layers by type of payload.
		/// </summary>
		/// <typeparam name="TPayload">The type of the payload.</typeparam>
		/// <returns></returns>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-11-20
		/// </remarks>
		IFilteredVisualLayerCollection FilterLayers<TPayload>() where TPayload : IPayload;

		/// <summary>
		/// Gets the contract time for the visual layers intersecting the given date time period.
		/// </summary>
		/// <param name="filterPeriod">The period of interest.</param>
		/// <returns></returns>
		TimeSpan ContractTime(DateTimePeriod filterPeriod);

		/// <summary>
		/// Gets the overtime for the visual layers intersecting the given date time period.
		/// </summary>
		/// <param name="filterPeriod">The period of interest.</param>
		/// <returns></returns>
		TimeSpan Overtime(DateTimePeriod filterPeriod);

		TimeSpan PaidTime(DateTimePeriod filterPeriod);
		TimeSpan WorkTime(DateTimePeriod filterPeriod);
		TimeSpan PlannedOvertime(DateTimePeriod filterPeriod);
	}

	///<summary>
	/// A resulting class for filtered visual layer collections.
	///</summary>
	public interface IFilteredVisualLayerCollection : IVisualLayerCollection
	{
		///<summary>
		/// Gets the original projection period this data was filtered from.
		///</summary>
		DateTimePeriod? OriginalProjectionPeriod { get; }
	}
}