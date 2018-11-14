using System;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy.Core
{
	public interface IPasswordManager
	{
		bool SendResetPasswordRequest(string userIdentifier, string baseUri);
		bool ValidateResetToken(string resetToken);
		void Modify(Guid personId, string oldPassword, string newPassword);
		bool Reset(string newPassword, string resetToken);
	}
}