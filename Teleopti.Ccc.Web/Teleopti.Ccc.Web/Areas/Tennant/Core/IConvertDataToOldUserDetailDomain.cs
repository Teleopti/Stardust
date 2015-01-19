using System;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	//will be deleted later
	public interface IConvertDataToOldUserDetailDomain
	{
		UserDetail Convert(int invalidAttempts, DateTime? invalidAttemptsSequenceStart, DateTime? lastPasswordChange);
	}
}