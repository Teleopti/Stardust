using System.Threading.Tasks;
using Microsoft.PowerBI.Api.V2;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IPowerBiClientFactory
	{
		Task<IPowerBIClient> CreatePowerBiClient();
	}
}