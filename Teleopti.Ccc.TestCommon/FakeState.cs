using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;


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