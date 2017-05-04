using Autofac;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

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

            if (_configuration.Toggle(Toggles.StaffingActions_RemoveScheduleForecastSkillChangeReadModel_43388))
            {
                builder.RegisterType<UpdateStaffingLevelReadModelOnlySkillCombinationResources>().As<IUpdateStaffingLevelReadModel>().InstancePerLifetimeScope();
            }
            else
            {
                builder.RegisterType<UpdateStaffingLevelReadModel>().As<IUpdateStaffingLevelReadModel>().InstancePerLifetimeScope();
            }
			
            if (_configuration.Toggle(Toggles.Staffing_ReadModel_Update14Days_43840))
            {
	            builder.RegisterType<StaffingSettingsReader>().As<IStaffingSettingsReader>().SingleInstance();
			}
            else
            {
	            builder.RegisterType<StaffingSettingsReader1Day>().As<IStaffingSettingsReader>().SingleInstance();
			}
			builder.RegisterType<ScheduleStaffingPossibilityCalculator>().As<IScheduleStaffingPossibilityCalculator>().SingleInstance();
			builder.RegisterType<SkillStaffingDataLoader>().As<ISkillStaffingDataLoader>().SingleInstance();
		}
    }
}