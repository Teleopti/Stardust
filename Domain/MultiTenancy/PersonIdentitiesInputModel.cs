using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class PersonIdentitiesInputModel
	{
		public List<PersonIdentityModel> People { get; set; }
		public string TimeStamp { get; set; }
		public string Intent { get; set; }
	}
}
