using System.Threading.Tasks;
using Microsoft.PowerBI.Api.V2;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;

namespace Teleopti.Ccc.WebTest.Areas.Insights.Core.PowerBi
{
	public class FakePowerBiClientFactory : IPowerBiClientFactory
	{
		private readonly IPowerBIClient _powerBiClient;

		public FakePowerBiClientFactory(IPowerBIClient powerBiClient)
		{
			_powerBiClient = powerBiClient;
		}

		public async Task<IPowerBIClient> CreatePowerBiClient()
		{
			return await Task.FromResult(_powerBiClient);
		}
	}
}