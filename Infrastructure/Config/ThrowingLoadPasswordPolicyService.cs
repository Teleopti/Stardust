﻿using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Config
{
	public class ThrowingLoadPasswordPolicyService : ILoadPasswordPolicyService
	{
		public TimeSpan LoadInvalidAttemptWindow()
		{
			throw new NotImplementedException("not to be used in runtime");
		}

		public int LoadMaxAttemptCount()
		{
			throw new NotImplementedException("not to be used in runtime");
		}

		public int LoadPasswordValidForDayCount()
		{
			throw new NotImplementedException("not to be used in runtime");
		}

		public int LoadPasswordExpireWarningDayCount()
		{
			throw new NotImplementedException("not to be used in runtime");
		}

		public IList<IPasswordStrengthRule> LoadPasswordStrengthRules()
		{
			throw new NotImplementedException("not to be used in runtime");
		}

		public string Path { get; set; }
		public void ClearFile()
		{
		}
		public string DocumentAsString { get; private set; }
	}
}