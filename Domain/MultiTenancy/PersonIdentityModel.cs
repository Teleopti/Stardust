using System;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class PersonIdentityModel
	{
		public string Identity { get; set; }
		public Guid PersonId { get; set; }
	}
}
