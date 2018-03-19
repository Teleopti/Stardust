using Autofac;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Core.Data;

namespace Teleopti.Ccc.Web.Core.IoC
{
	public class WebCommonModule : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<ActivityProvider>().As<IActivityProvider>().SingleInstance();
			builder.RegisterType<ShiftCategoryProvider>().As<IShiftCategoryProvider>().SingleInstance();
			builder.RegisterType<MultiplicatorDefinitionSetProvider>().As<IMultiplicatorDefinitionSetProvider>().SingleInstance();
			builder.RegisterType<MultipartHttpContentExtractor>().As<IMultipartHttpContentExtractor>().SingleInstance();
		}
	}
}