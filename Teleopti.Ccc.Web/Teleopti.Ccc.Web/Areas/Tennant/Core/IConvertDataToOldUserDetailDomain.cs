﻿using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.Tennant.Core
{
	//will be deleted later
	public interface IConvertDataToOldUserDetailDomain
	{
		UserDetail Convert(PasswordPolicyForUser passwordPolicyForUser);
	}
}