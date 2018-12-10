using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.MultiTenancy
{
	public class PersonInfoModel : IPersonInfoModel
	{
		public string ApplicationLogonName { get; set; }
		public string Password { get; set; }
		public string Identity { get; set; }
		public Guid PersonId { get; set; }
	}
}