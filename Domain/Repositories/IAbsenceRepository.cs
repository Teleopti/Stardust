using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
	public interface IAbsenceRepository : IRepository<IAbsence>
	{
		/// <summary>
		/// Loads the All absences by sorting it's by name.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-10-05
		/// </remarks>
		IEnumerable<IAbsence> LoadAllSortByName();

		/// <summary>
		/// Loads Requestable absences
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: Sumedah
		/// Created date: 2008-10-07
		/// </remarks>
		IList<IAbsence> LoadRequestableAbsence();

		/// <summary>
		/// Gets the absences with trackers that are used in person accounts.
		/// </summary>
		/// <returns></returns>
		/// <remarks>
		/// Created by: jonas n
		/// Created date: 2010-03-02
		/// </remarks>
		IList<IAbsence> FindAbsenceTrackerUsedByPersonAccount();
	}
}
