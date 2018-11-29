using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface ILoadPasswordPolicyService
	{
		TimeSpan LoadInvalidAttemptWindow();
		int LoadMaxAttemptCount();
		int LoadPasswordValidForDayCount();
		int LoadPasswordExpireWarningDayCount();
		IList<IPasswordStrengthRule> LoadPasswordStrengthRules();
		string Path { get; set; }
		void ClearFile();
		string DocumentAsString { get; }
	}
}
