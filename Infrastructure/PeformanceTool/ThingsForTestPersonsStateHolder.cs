using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.PeformanceTool
{
	public class ThingsForTestPersonsStateHolder
	{
		public IList<IPerson> Persons = new List<IPerson>();
		public IList<IExternalLogOn> ExternalLogOns = new List<IExternalLogOn>();
		public IList<IPersonPeriod> PersonPeriods = new List<IPersonPeriod>();

		public ISite Site { get; set; }
		public ITeam Team { get; set; }
		public IPartTimePercentage PartTimePercentage { get; set; }
		public IContract Contract { get; set; }
		public IContractSchedule ContractSchedule { get; set; }
	}
}