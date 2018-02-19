using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

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


			builder.RegisterType<UpdateStaffingLevelReadModelOnlySkillCombinationResources>().InstancePerLifetimeScope();

			
			builder.RegisterType<StaffingSettingsReader>().As<IStaffingSettingsReader>().SingleInstance();
			
			if (_configuration.Toggle((Toggles.Wfm_Staffing_StaffingReadModel28DaysStep1_45109)))
			{
				builder.RegisterType<StaffingSettingsReader28Days>().As<IStaffingSettingsReader>().SingleInstance();
			}
			if (_configuration.Toggle((Toggles.Wfm_Staffing_StaffingReadModel49DaysStep2_45109)))
			{
				builder.RegisterType<StaffingSettingsReader49Days>().As<IStaffingSettingsReader>().SingleInstance();
			}

			builder.RegisterType<ScheduleStaffingPossibilityCalculator>().As<IScheduleStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffingDataLoader>().As<ISkillStaffingDataLoader>().SingleInstance();
			builder.RegisterType<ScheduledStaffingViewModelCreator>().SingleInstance();
			builder.RegisterType<ImportBpoFile>().SingleInstance();
			
			if(_configuration.Toggle(Toggles.Forecast_FileImport_UnifiedFormat_46585))
				builder.RegisterType<ExportBpoFile>().As<IExportBpoFile>();
			else
				builder.RegisterType<ExportBpoFileOld>().As<IExportBpoFile>();
			
			builder.RegisterType<ExportForecastAndStaffingFile>().SingleInstance();
			
			if (_configuration.Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686))
			{
				builder.RegisterType<PrimaryPersonSkillFilter>().As<IPrimaryPersonSkillFilter>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PrimaryPersonSkillFilterToggle44686Off>().As<IPrimaryPersonSkillFilter>().SingleInstance();
			}
		}
	}
}