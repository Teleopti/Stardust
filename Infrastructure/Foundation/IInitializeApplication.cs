using System.Collections.Generic;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	public interface IInitializeApplication
	{
		void Start(IState clientCache, ILoadPasswordPolicyService loadPasswordPolicyService, IDictionary<string, string> appSettings);
	}
}