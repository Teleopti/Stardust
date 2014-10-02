using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence
{
	public class RtaAggregationData
	{
		public PersonOrganizationData OrganizationData { get; set; }
		public IActualAgentState ActualAgentState { get; set; }
	}
}