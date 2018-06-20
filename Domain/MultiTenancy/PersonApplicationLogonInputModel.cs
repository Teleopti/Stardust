using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class PersonApplicationLogonInputModel
	{
		public List<PersonApplicationLogonModel> People { get; set; }
		public string TimeStamp { get; set; }
		public string Intent { get; set; }
	}
}