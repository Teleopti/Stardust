using Autofac;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.IoC
{
	public class OutboundAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundCampaignPersister>().As<IOutboundCampaignPersister>().SingleInstance();
		}
	}
}