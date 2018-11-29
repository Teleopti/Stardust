using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IPersonInfoModel
	{
		string ApplicationLogonName { get; set; }
		string Password { get; set; }
		string Identity { get; set; }
		Guid PersonId { get; set; }
	}
}