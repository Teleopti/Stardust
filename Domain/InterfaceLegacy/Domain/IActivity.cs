using System.Collections.Generic;
using System.Drawing;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// An activity
	/// </summary>
	/// <remarks>
	/// Created by: rogerkr
	/// Created date: 2008-08-07
	/// </remarks>
	public interface IActivity : IPayload, IDeleteTag
	{
		IList<IActivity> ActivityCollection { get; }

		/// <summary>
		/// Gets or sets the name.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// Created by: robink
		/// Created date: 2007-10-26
		/// </remarks>
		Description Description
		{
			get;
			set;
		}

		/// <summary>
		/// Gets the display color of the Payload.
		/// </summary>
		/// <value>The color of the display.</value>
		/// <remarks>No property later - methods instead!</remarks>
		Color DisplayColor
		{
			get;
			set;
		}
		/// <summary>
		/// Gets the name part of the description.
		/// </summary>
		/// <value>The name.</value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-05-26
		/// </remarks>
		string Name { get; }

		/// <summary>
		/// Gets or sets a value indicating whether this activity is considered to be counted as ready time.
		/// </summary>
		/// <value>
		/// 	<c>true</c> if this instance is ready time; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>
		/// Created by: micke
		/// Created date: 2008-08-13
		/// </remarks>
		bool InReadyTime { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [requires skill].
		/// </summary>
		/// <value><c>true</c> if [requires skill]; otherwise, <c>false</c>.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2008-11-13    
		/// /// </remarks>
		bool RequiresSkill { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Activity is calculated as work time.
		/// </summary>
		/// <value><c>true</c> if [work time]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-11-20
		/// </remarks>
		bool InWorkTime { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether Activity is calculated as paid time.
		/// </summary>
		/// <value><c>true</c> if [paid time]; otherwise, <c>false</c>.</value>
		/// <remarks>
		/// Created by: zoet
		/// Created date: 2008-11-20
		/// </remarks>
		bool InPaidTime { get; set; }

		/// <summary>
		/// Gets or sets the short break or lunch.
		/// </summary>
		/// <value>The short break or lunch.</value>
		/// /// 
		/// <remarks>
		///  Created by: Ola
		///  Created date: 2009-05-07    
		/// /// </remarks>
		ReportLevelDetail ReportLevelDetail { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether [requires seat].
		/// </summary>
		/// <value><c>true</c> if [requires seat]; otherwise, <c>false</c>.</value>
		bool RequiresSeat { get; set; }

		///<summary>
		/// Gets or sets the PayrollCode
		///</summary>
		string PayrollCode { get; set; }

		/// <summary>
		/// Allow the overwrite on the activity
		/// </summary>
		bool AllowOverwrite { get; set; }

		/// <summary>
		/// Indicates if this is an implicit activity and should be hidden in options for example
		/// </summary>
		bool IsOutboundActivity { get; set; }
	}
}
