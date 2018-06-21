using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	public class StaffingModule : Module
	{
		private readonly IIocConfiguration _configuration;

		public StaffingModule(IIocConfiguration configuration)
		{
			_configuration = configuration;
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<CompareProjection>().SingleInstance();

			builder.RegisterType<UpdateStaffingLevelReadModelOnlySkillCombinationResources>().As<IUpdateStaffingLevelReadModel>().InstancePerLifetimeScope();
			builder.RegisterType<StaffingSettingsReader49Days>().As<IStaffingSettingsReader>().SingleInstance();
			
			builder.RegisterType<SkillStaffingDataLoader>().As<ISkillStaffingDataLoader>().SingleInstance();
			builder.RegisterType<ScheduledStaffingViewModelCreator>().SingleInstance();
			builder.RegisterType<BacklogSkillTypesForecastCalculator>().SingleInstance();
			builder.RegisterType<ImportBpoFile>().SingleInstance();
			builder.RegisterType<SkillCombinationBpoTimeLineReader>().As<ISkillCombinationBpoTimeLineReader>().SingleInstance();
			builder.RegisterType<BpoProvider>().SingleInstance();
			builder.RegisterType<ExportBpoFile>().As<IExportBpoFile>();
			
			builder.RegisterType<ExportForecastAndStaffingFile>().SingleInstance();
			builder.RegisterType<ExportStaffingPeriodValidationProvider>().As<ExportStaffingPeriodValidationProvider>().SingleInstance();
		}
	}
}