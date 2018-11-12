using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeState : IState
	{
		public IApplicationData ApplicationScopeData { get; set; }

		public void SetApplicationData(IApplicationData applicationData)
		{
			ApplicationScopeData = applicationData;
		}
	}
}