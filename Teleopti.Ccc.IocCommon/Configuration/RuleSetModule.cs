using Autofac;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class RuleSetModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public RuleSetModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}
		
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CreateWorkShiftsFromTemplate>().As<ICreateWorkShiftsFromTemplate>().SingleInstance();
			builder.RegisterType<ShiftCreatorService>().As<IShiftCreatorService>().SingleInstance();
			builder.CacheByInterfaceProxy<RuleSetProjectionService, IRuleSetProjectionService>().SingleInstance();
			builder.CacheByInterfaceProxy<RuleSetProjectionEntityService, IRuleSetProjectionEntityService>().SingleInstance();
			builder.CacheByInterfaceProxy<WorkShiftWorkTime, IWorkShiftWorkTime>().SingleInstance();

			_configuration.Cache().This<IRuleSetProjectionService>(b => b
					.CacheMethod(m => m.ProjectionCollection(null, null))
				, "RSPS");

			_configuration.Cache().This<IRuleSetProjectionEntityService>(b => b
					.CacheMethod(m => m.ProjectionCollection(null, null))
				, "RSPES");

			_configuration.Cache().This<IWorkShiftWorkTime>(b => b
					.CacheMethod(m => m.CalculateMinMax(null, null))
				, "WSWT");
		}
	}
}