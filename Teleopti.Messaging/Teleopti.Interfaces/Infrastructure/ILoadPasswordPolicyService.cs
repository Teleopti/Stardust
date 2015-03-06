﻿using System;
using System.Collections.Generic;
using System.Xml;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
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
