using Autofac;
using Teleopti.Ccc.Domain.Outbound;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.DataProvider;
using Teleopti.Ccc.Web.Areas.Outbound.core.Campaign.Mapping;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Outbound.core.IoC
{
	public class OutboundAreaModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<OutboundCampaignPersister>().As<IOutboundCampaignPersister>().SingleInstance();
			builder.RegisterType<OutboundCampaignViewModelMapper>().As<IOutboundCampaignViewModelMapper>().SingleInstance();
			builder.RegisterType<OutboundCampaignMapper>().As<IOutboundCampaignMapper>().SingleInstance();
			builder.RegisterType<OutboundSkillCreator>().As<OutboundSkillCreator>().SingleInstance();
			builder.RegisterType<OutboundSkillTypeProvider>().As<IOutboundSkillTypeProvider>().SingleInstance();
		}
	}
}