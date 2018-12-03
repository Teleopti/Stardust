using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public class ApplicationData : IApplicationData
	{
		public ApplicationData(
			IDictionary<string, string> appSettings,
			ILoadPasswordPolicyService loadPasswordPolicyService)
		{
			AppSettings = appSettings;
			LoadPasswordPolicyService = loadPasswordPolicyService;
		}

		public ILoadPasswordPolicyService LoadPasswordPolicyService { get; }

		public IDictionary<string, string> AppSettings { get; }

		public void Dispose()
		{
		}
	}
}