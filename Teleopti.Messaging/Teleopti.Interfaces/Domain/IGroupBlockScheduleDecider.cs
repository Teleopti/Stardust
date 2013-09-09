using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Determins if a block can be scheduled using group scheduling
	/// </summary>
	public interface IGroupBlockScheduleDecider
	{
		/// <summary>
		/// Finds a legal block for the provided date and GroupPerson
		/// </summary>
		/// <param name="dateOnly">The date only.</param>
		/// <param name="groupPerson">The group person.</param>
		/// <returns></returns>
		IList<DateOnly> Execute(DateOnly dateOnly, IGroupPerson groupPerson);
	}
}