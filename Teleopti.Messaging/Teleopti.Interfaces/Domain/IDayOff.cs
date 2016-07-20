using System;
using System.Drawing;

namespace Teleopti.Interfaces.Domain
{
	public interface IDayOff 
	{
		/// <summary>
		/// Gets the flexibility.
		/// </summary>
		TimeSpan Flexibility { get; }

		/// <summary>
		/// Gets the length (duration).
		/// </summary>
		TimeSpan TargetLength { get; }

		/// <summary>
		/// Gets the anchor point as date and time.
		/// </summary>
		DateTime Anchor { get; }

		/// <summary>
		/// Description
		/// </summary>
		Description Description { get; }

		/// <summary>
		/// Gets the displayColor
		/// </summary>
		Color DisplayColor { get; }

		/// <summary>
		/// Gets the boundary.
		/// </summary>
		/// <value>The boundary.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-11-08
		/// </remarks>
		DateTimePeriod Boundary { get; }

		/// <summary>
		/// Gets the inner boundary.
		/// </summary>
		/// <value>The inner boundary.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-06-12    
		/// /// </remarks>
		DateTimePeriod InnerBoundary { get; }

		///<summary>
		/// Gets the payrollcode
		///</summary>
		string PayrollCode { get; }

		/// <summary>
		/// Gets the anchor with current time zone applied.
		/// </summary>
		/// <value>The anchor local.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-22
		/// </remarks>
		DateTime AnchorLocal(TimeZoneInfo targetTimeZone);
	}
}