using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IForecastingTarget
	{
		/// <summary>
		/// Gets the current date.
		/// </summary>
		/// <value>The current date.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		DateOnly CurrentDate { get; }

		/// <summary>
		/// Gets or sets the tasks.
		/// </summary>
		/// <value>The tasks.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-03-04
		/// </remarks>
		double Tasks { get; set; }

		/// <summary>
		/// Gets or sets the average after task time.
		/// </summary>
		/// <value>The average after task time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-17
		/// </remarks>
		TimeSpan AverageAfterTaskTime { get; set; }

		/// <summary>
		/// Gets or sets the average task time.
		/// </summary>
		/// <value>The average task time.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-12-17
		/// </remarks>
		TimeSpan AverageTaskTime { get; set; }

		/// <summary>
		/// Gets a value indicating whether this instance is closed.
		/// </summary>
		/// <value><c>true</c> if this instance is closed; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2008-01-25
		/// </remarks>
		OpenForWork OpenForWork { get; }
	}
}