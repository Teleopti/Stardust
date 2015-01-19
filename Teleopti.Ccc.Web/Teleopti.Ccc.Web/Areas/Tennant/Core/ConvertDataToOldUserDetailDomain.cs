using System;
using Teleopti.Ccc.Domain.Security;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	public class ConvertDataToOldUserDetailDomain : IConvertDataToOldUserDetailDomain
	{
		public UserDetail Convert(int invalidAttempts, DateTime? invalidAttemptsSequenceStart, DateTime? lastPasswordChange)
		{
			if (invalidAttemptsSequenceStart.HasValue)
			{
				return new UserDetail(null)
				{
					InvalidAttempts = invalidAttempts,
					InvalidAttemptsSequenceStart = invalidAttemptsSequenceStart.Value,
					LastPasswordChange = lastPasswordChange.Value
				};
			}
			return new UserDetail(null);
		}
	}
}