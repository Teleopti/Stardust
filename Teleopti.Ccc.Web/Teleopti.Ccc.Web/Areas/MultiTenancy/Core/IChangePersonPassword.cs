using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IChangePersonPassword
	{
		void Modify(Guid personId, string oldPassword, string newPassword);
	}
}