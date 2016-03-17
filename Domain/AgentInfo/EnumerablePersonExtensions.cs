using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public static class EnumerablePersonExtensions
	{
		public static IEnumerable<IPerson> FixedStaffPeople(this IEnumerable<IPerson> agents, DateOnlyPeriod period)
		{
			return agents.Where(
				p =>
					p.PersonPeriods(period)
						.Any(
							pp =>
								pp.PersonContract != null && pp.PersonContract.Contract != null &&
								pp.PersonContract.Contract.EmploymentType != EmploymentType.HourlyStaff));
		}
	}
}