using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Start.Core.Config
{
	public class FakeLoadPasswordPolicyService : ILoadPasswordPolicyService
	{
		public string Path { get; set; }
		public string DocumentAsString { get; set; }

		public TimeSpan LoadInvalidAttemptWindow()
		{
			return new TimeSpan();
		}

		public int LoadMaxAttemptCount()
		{
			return 0;
		}

		public int LoadPasswordValidForDayCount()
		{
			return 0;
		}

		public int LoadPasswordExpireWarningDayCount()
		{
			return 0;
		}

		public IList<IPasswordStrengthRule> LoadPasswordStrengthRules()
		{
			return null;
		}
	}
}