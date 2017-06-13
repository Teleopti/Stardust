using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters
{
	public class OrganizationModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<OrganizationTeamModel> Teams;
	}

	public class OrganizationTeamModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
	}

}