using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ITaskOwnerPeriod : ITaskOwner
	{
		/// <summary>
		/// Gets the end date of the current month for the thread culture.
		/// </summary>
		/// <value>The end date.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-17
		/// </remarks>
		DateOnly EndDate { get; }

		/// <summary>
		/// Gets a value indicating whether this instance is loaded.
		/// </summary>
		/// <value><c>true</c> if this instance is loaded; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-17
		/// </remarks>
		bool IsLoaded { get; }

		/// <summary>
		/// Gets the start date of the month of the current date for the thread culture.
		/// </summary>
		/// <value>The start date.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-17
		/// </remarks>
		DateOnly StartDate { get; }

		/// <summary>
		/// Gets the task owner day collection.
		/// </summary>
		/// <value>The task owner day collection.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		ReadOnlyCollection<ITaskOwner> TaskOwnerDayCollection { get; }

		/// <summary>
		/// Gets or sets the type of task owner period.
		/// </summary>
		/// <value>The type of task owner period.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		TaskOwnerPeriodType TypeOfTaskOwnerPeriod { get; set; }

		/// <summary>
		/// Gets the forecasted incoming demand.
		/// </summary>
		/// <value>The forecasted incoming demand.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-05
		/// </remarks>
		TimeSpan ForecastedIncomingDemand { get; }

		/// <summary>
		/// Gets the forecasted incoming demand with shrinkage.
		/// </summary>
		/// <value>The forecasted incoming demand with shrinkage.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-09-05
		/// </remarks>
		TimeSpan ForecastedIncomingDemandWithShrinkage { get; }

		/// <summary>
		/// Adds the specified task owner day.
		/// </summary>
		/// <param name="taskOwnerDay">The task owner day.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		void Add(ITaskOwner taskOwnerDay);

		/// <summary>
		/// Adds the range.
		/// </summary>
		/// <param name="taskOwnerDays">The task owner days.</param>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		void AddRange(IEnumerable<ITaskOwner> taskOwnerDays);

		/// <summary>
		/// Clears this instance.
		/// </summary>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		void Clear();
	}
}