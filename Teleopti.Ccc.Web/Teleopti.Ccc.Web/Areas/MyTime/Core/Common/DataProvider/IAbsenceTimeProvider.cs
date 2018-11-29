using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{	
	/// <summary>
	/// Responsible for getting the total time of used absence for a certain period
	/// </summary>
	/// <remarks>
	/// Created by: marias	
	/// Created date: 2013-04-22
	/// </remarks>
	public interface IAbsenceTimeProvider
	{
		/// <summary>
		/// Returns a list of dates with the total absencetime in minutes for each day
		/// </summary>
		/// <param name="period"></param>
		/// <returns></returns>
		IEnumerable<IAbsenceAgents> GetAbsenceTimeForPeriod(DateOnlyPeriod period);
	}
}