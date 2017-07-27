using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class StaffingModule : Module
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

			if (_configuration.Toggle(Toggles.Staffing_ReadModel_Keep8DaysHistoricalData_44652))
			{
				builder.RegisterType<StaffingSettingsReader>().As<IStaffingSettingsReader>().SingleInstance();
			}
			else if (_configuration.Toggle(Toggles.Staffing_ReadModel_Update14Days_43840))
			{
				builder.RegisterType<StaffingSettingsReaderNoHistorical>().As<IStaffingSettingsReader>().SingleInstance();
			}
			else
			{
				builder.RegisterType<StaffingSettingsReader1Day>().As<IStaffingSettingsReader>().SingleInstance();
			}
			builder.RegisterType<ScheduleStaffingPossibilityCalculator>().As<IScheduleStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffingDataLoader>().As<ISkillStaffingDataLoader>().SingleInstance();
			builder.RegisterType<ScheduledStaffingViewModelCreator>().SingleInstance();
			builder.RegisterType<ImportBpoFile>().SingleInstance();

			if (_configuration.Toggle(Toggles.MyTimeWeb_CalculateOvertimeProbabilityByPrimarySkill_44686))
			{
				builder.RegisterType<PrimaryPersonSkillFilter>().As<IPrimaryPersonSkillFilter>().SingleInstance();
			}
			else
			{
				builder.RegisterType<PrimaryPersonSkillFilterToggle44686Off>().As<IPrimaryPersonSkillFilter>().SingleInstance();
			}
			builder.RegisterType<SkillCombinationResourceBpoRepository>().As<ISkillCombinationResourceBpoRepository>().SingleInstance();
		}
	}
}