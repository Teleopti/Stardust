using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Teleopti.Analytics.Etl.IntegrationTest.Models.Mapping;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class AnalyticsContext : DbContext
    {
        static AnalyticsContext()
        {
            Database.SetInitializer<AnalyticsContext>(null);
        }

        public AnalyticsContext(string connectionString)
            : base("Name=AnalyticsContext")
        {
            Database.Connection.ConnectionString = connectionString;
        }

        public DbSet<acd_type> acd_type { get; set; }
        public DbSet<acd_type_detail> acd_type_detail { get; set; }
        public DbSet<AdvancedLoggingService> AdvancedLoggingServices { get; set; }
        public DbSet<agent_info> agent_info { get; set; }
        public DbSet<agent_logg> agent_logg { get; set; }
        public DbSet<aspnet_applications> aspnet_applications { get; set; }
        public DbSet<aspnet_Membership> aspnet_Membership { get; set; }
        public DbSet<aspnet_SchemaVersions> aspnet_SchemaVersions { get; set; }
        public DbSet<aspnet_Users> aspnet_Users { get; set; }
        public DbSet<ccc_system_info> ccc_system_info { get; set; }
        public DbSet<DatabaseVersion> DatabaseVersions { get; set; }
        public DbSet<DBA_VirtualFileStats> DBA_VirtualFileStats { get; set; }
        public DbSet<DBA_VirtualFileStatsHistory> DBA_VirtualFileStatsHistory { get; set; }
        public DbSet<log_object> log_object { get; set; }
        public DbSet<log_object_add_hours> log_object_add_hours { get; set; }
        public DbSet<log_object_detail> log_object_detail { get; set; }
        public DbSet<quality_info> quality_info { get; set; }
        public DbSet<quality_logg> quality_logg { get; set; }
        public DbSet<queue_logg> queue_logg { get; set; }
        public DbSet<queue> queues { get; set; }
        public DbSet<adherence_calculation> adherence_calculation { get; set; }
        public DbSet<bridge_acd_login_person> bridge_acd_login_person { get; set; }
        public DbSet<bridge_group_page_person> bridge_group_page_person { get; set; }
        public DbSet<bridge_queue_workload> bridge_queue_workload { get; set; }
        public DbSet<bridge_skillset_skill> bridge_skillset_skill { get; set; }
        public DbSet<bridge_time_zone> bridge_time_zone { get; set; }
        public DbSet<custom_report> custom_report { get; set; }
        public DbSet<custom_report_control> custom_report_control { get; set; }
        public DbSet<custom_report_control_collection> custom_report_control_collection { get; set; }
        public DbSet<dim_absence> dim_absence { get; set; }
        public DbSet<dim_acd_login> dim_acd_login { get; set; }
        public DbSet<dim_activity> dim_activity { get; set; }
        public DbSet<dim_business_unit> dim_business_unit { get; set; }
        public DbSet<dim_date> dim_date { get; set; }
        public DbSet<dim_day_off> dim_day_off { get; set; }
        public DbSet<dim_group_page> dim_group_page { get; set; }
        public DbSet<dim_interval> dim_interval { get; set; }
        public DbSet<dim_kpi> dim_kpi { get; set; }
        public DbSet<dim_overtime> dim_overtime { get; set; }
        public DbSet<dim_person> dim_person { get; set; }
        public DbSet<dim_preference_type> dim_preference_type { get; set; }
        public DbSet<dim_quality_quest> dim_quality_quest { get; set; }
        public DbSet<dim_queue> dim_queue { get; set; }
        public DbSet<dim_queue_excluded> dim_queue_excluded { get; set; }
        public DbSet<dim_request_status> dim_request_status { get; set; }
        public DbSet<dim_request_type> dim_request_type { get; set; }
        public DbSet<dim_scenario> dim_scenario { get; set; }
        public DbSet<dim_scorecard> dim_scorecard { get; set; }
        public DbSet<dim_shift_category> dim_shift_category { get; set; }
        public DbSet<dim_shift_length> dim_shift_length { get; set; }
        public DbSet<dim_site> dim_site { get; set; }
        public DbSet<dim_skill> dim_skill { get; set; }
        public DbSet<dim_skillset> dim_skillset { get; set; }
        public DbSet<dim_team> dim_team { get; set; }
        public DbSet<dim_time_zone> dim_time_zone { get; set; }
        public DbSet<dim_workload> dim_workload { get; set; }
        public DbSet<etl_job> etl_job { get; set; }
        public DbSet<etl_job_delayed> etl_job_delayed { get; set; }
        public DbSet<etl_job_execution> etl_job_execution { get; set; }
        public DbSet<etl_job_schedule> etl_job_schedule { get; set; }
        public DbSet<etl_job_schedule_period> etl_job_schedule_period { get; set; }
        public DbSet<etl_jobstep> etl_jobstep { get; set; }
        public DbSet<etl_jobstep_error> etl_jobstep_error { get; set; }
        public DbSet<etl_jobstep_execution> etl_jobstep_execution { get; set; }
        public DbSet<etl_maintenance_configuration> etl_maintenance_configuration { get; set; }
        public DbSet<fact_agent> fact_agent { get; set; }
        public DbSet<fact_agent_queue> fact_agent_queue { get; set; }
        public DbSet<fact_forecast_workload> fact_forecast_workload { get; set; }
        public DbSet<fact_hourly_availability> fact_hourly_availability { get; set; }
        public DbSet<fact_kpi_targets_team> fact_kpi_targets_team { get; set; }
        public DbSet<fact_quality> fact_quality { get; set; }
        public DbSet<fact_queue> fact_queue { get; set; }
        public DbSet<fact_request> fact_request { get; set; }
        public DbSet<fact_requested_days> fact_requested_days { get; set; }
        public DbSet<fact_schedule> fact_schedule { get; set; }
        public DbSet<fact_schedule_day_count> fact_schedule_day_count { get; set; }
        public DbSet<fact_schedule_deviation> fact_schedule_deviation { get; set; }
        public DbSet<fact_schedule_forecast_skill> fact_schedule_forecast_skill { get; set; }
        public DbSet<fact_schedule_preference> fact_schedule_preference { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<language_translation> language_translation { get; set; }
        public DbSet<period_type> period_type { get; set; }
        public DbSet<permission_report> permission_report { get; set; }
        public DbSet<pm_user> pm_user { get; set; }
        public DbSet<report> reports { get; set; }
        public DbSet<report_control> report_control { get; set; }
        public DbSet<report_control_collection> report_control_collection { get; set; }
        public DbSet<report_user_setting> report_user_setting { get; set; }
        public DbSet<Report1> Reports1 { get; set; }
        public DbSet<scorecard_kpi> scorecard_kpi { get; set; }
        public DbSet<service_level_calculation> service_level_calculation { get; set; }
        public DbSet<sys_configuration> sys_configuration { get; set; }
        public DbSet<sys_crossdatabaseview> sys_crossdatabaseview { get; set; }
        public DbSet<sys_crossdatabaseview_target> sys_crossdatabaseview_target { get; set; }
        public DbSet<sys_datasource> sys_datasource { get; set; }
        public DbSet<sys_etl_running_lock> sys_etl_running_lock { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessagesPurged> MessagesPurgeds { get; set; }
        public DbSet<Queues1> Queues1 { get; set; }
        public DbSet<SubscriptionStorage> SubscriptionStorages { get; set; }
        public DbSet<ActualAgentState> ActualAgentStates { get; set; }
        public DbSet<ExternalAgentState> ExternalAgentStates { get; set; }
        public DbSet<stg_absence> stg_absence { get; set; }
        public DbSet<stg_activity> stg_activity { get; set; }
        public DbSet<stg_agent_skill> stg_agent_skill { get; set; }
        public DbSet<stg_agent_skillset> stg_agent_skillset { get; set; }
        public DbSet<stg_business_unit> stg_business_unit { get; set; }
        public DbSet<stg_date> stg_date { get; set; }
        public DbSet<stg_day_off> stg_day_off { get; set; }
        public DbSet<stg_forecast_workload> stg_forecast_workload { get; set; }
        public DbSet<stg_group_page_person> stg_group_page_person { get; set; }
        public DbSet<stg_hourly_availability> stg_hourly_availability { get; set; }
        public DbSet<stg_kpi> stg_kpi { get; set; }
        public DbSet<stg_kpi_targets_team> stg_kpi_targets_team { get; set; }
        public DbSet<stg_overtime> stg_overtime { get; set; }
        public DbSet<stg_permission_report> stg_permission_report { get; set; }
        public DbSet<stg_person> stg_person { get; set; }
        public DbSet<stg_queue> stg_queue { get; set; }
        public DbSet<stg_queue_workload> stg_queue_workload { get; set; }
        public DbSet<stg_request> stg_request { get; set; }
        public DbSet<stg_scenario> stg_scenario { get; set; }
        public DbSet<stg_schedule> stg_schedule { get; set; }
        public DbSet<stg_schedule_changed> stg_schedule_changed { get; set; }
        public DbSet<stg_schedule_day_absence_count> stg_schedule_day_absence_count { get; set; }
        public DbSet<stg_schedule_day_off_count> stg_schedule_day_off_count { get; set; }
        public DbSet<stg_schedule_forecast_skill> stg_schedule_forecast_skill { get; set; }
        public DbSet<stg_schedule_preference> stg_schedule_preference { get; set; }
        public DbSet<stg_schedule_updated_personLocal> stg_schedule_updated_personLocal { get; set; }
        public DbSet<stg_schedule_updated_ShiftStartDateUTC> stg_schedule_updated_ShiftStartDateUTC { get; set; }
        public DbSet<stg_scorecard> stg_scorecard { get; set; }
        public DbSet<stg_scorecard_kpi> stg_scorecard_kpi { get; set; }
        public DbSet<stg_shift_category> stg_shift_category { get; set; }
        public DbSet<stg_skill> stg_skill { get; set; }
        public DbSet<stg_time_zone> stg_time_zone { get; set; }
        public DbSet<stg_time_zone_bridge> stg_time_zone_bridge { get; set; }
        public DbSet<stg_user> stg_user { get; set; }
        public DbSet<stg_workload> stg_workload { get; set; }
        public DbSet<vw_aspnet_Applications> vw_aspnet_Applications { get; set; }
        public DbSet<vw_aspnet_MembershipUsers> vw_aspnet_MembershipUsers { get; set; }
        public DbSet<vw_aspnet_Users> vw_aspnet_Users { get; set; }
        public DbSet<v_agent_info> v_agent_info { get; set; }
        public DbSet<v_agent_logg> v_agent_logg { get; set; }
        public DbSet<v_bridge_acd_login_person_date> v_bridge_acd_login_person_date { get; set; }
        public DbSet<v_ccc_system_info> v_ccc_system_info { get; set; }
        public DbSet<v_log_object> v_log_object { get; set; }
        public DbSet<v_permission_report> v_permission_report { get; set; }
        public DbSet<v_queue_logg> v_queue_logg { get; set; }
        public DbSet<v_queues> v_queues { get; set; }
        public DbSet<v_report> v_report { get; set; }
        public DbSet<v_report_control> v_report_control { get; set; }
        public DbSet<v_report_control_collection> v_report_control_collection { get; set; }
        public DbSet<v_stg_queue> v_stg_queue { get; set; }
        public DbSet<v_time_zone_convertUpDown> v_time_zone_convertUpDown { get; set; }
        public DbSet<v_stg_schedule_load> v_stg_schedule_load { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new acd_typeMap());
            modelBuilder.Configurations.Add(new acd_type_detailMap());
            modelBuilder.Configurations.Add(new AdvancedLoggingServiceMap());
            modelBuilder.Configurations.Add(new agent_infoMap());
            modelBuilder.Configurations.Add(new agent_loggMap());
            modelBuilder.Configurations.Add(new aspnet_applicationsMap());
            modelBuilder.Configurations.Add(new aspnet_MembershipMap());
            modelBuilder.Configurations.Add(new aspnet_SchemaVersionsMap());
            modelBuilder.Configurations.Add(new aspnet_UsersMap());
            modelBuilder.Configurations.Add(new ccc_system_infoMap());
            modelBuilder.Configurations.Add(new DatabaseVersionMap());
            modelBuilder.Configurations.Add(new DBA_VirtualFileStatsMap());
            modelBuilder.Configurations.Add(new DBA_VirtualFileStatsHistoryMap());
            modelBuilder.Configurations.Add(new log_objectMap());
            modelBuilder.Configurations.Add(new log_object_add_hoursMap());
            modelBuilder.Configurations.Add(new log_object_detailMap());
            modelBuilder.Configurations.Add(new quality_infoMap());
            modelBuilder.Configurations.Add(new quality_loggMap());
            modelBuilder.Configurations.Add(new queue_loggMap());
            modelBuilder.Configurations.Add(new queueMap());
            modelBuilder.Configurations.Add(new adherence_calculationMap());
            modelBuilder.Configurations.Add(new bridge_acd_login_personMap());
            modelBuilder.Configurations.Add(new bridge_group_page_personMap());
            modelBuilder.Configurations.Add(new bridge_queue_workloadMap());
            modelBuilder.Configurations.Add(new bridge_skillset_skillMap());
            modelBuilder.Configurations.Add(new bridge_time_zoneMap());
            modelBuilder.Configurations.Add(new custom_reportMap());
            modelBuilder.Configurations.Add(new custom_report_controlMap());
            modelBuilder.Configurations.Add(new custom_report_control_collectionMap());
            modelBuilder.Configurations.Add(new dim_absenceMap());
            modelBuilder.Configurations.Add(new dim_acd_loginMap());
            modelBuilder.Configurations.Add(new dim_activityMap());
            modelBuilder.Configurations.Add(new dim_business_unitMap());
            modelBuilder.Configurations.Add(new dim_dateMap());
            modelBuilder.Configurations.Add(new dim_day_offMap());
            modelBuilder.Configurations.Add(new dim_group_pageMap());
            modelBuilder.Configurations.Add(new dim_intervalMap());
            modelBuilder.Configurations.Add(new dim_kpiMap());
            modelBuilder.Configurations.Add(new dim_overtimeMap());
            modelBuilder.Configurations.Add(new dim_personMap());
            modelBuilder.Configurations.Add(new dim_preference_typeMap());
            modelBuilder.Configurations.Add(new dim_quality_questMap());
            modelBuilder.Configurations.Add(new dim_queueMap());
            modelBuilder.Configurations.Add(new dim_queue_excludedMap());
            modelBuilder.Configurations.Add(new dim_request_statusMap());
            modelBuilder.Configurations.Add(new dim_request_typeMap());
            modelBuilder.Configurations.Add(new dim_scenarioMap());
            modelBuilder.Configurations.Add(new dim_scorecardMap());
            modelBuilder.Configurations.Add(new dim_shift_categoryMap());
            modelBuilder.Configurations.Add(new dim_shift_lengthMap());
            modelBuilder.Configurations.Add(new dim_siteMap());
            modelBuilder.Configurations.Add(new dim_skillMap());
            modelBuilder.Configurations.Add(new dim_skillsetMap());
            modelBuilder.Configurations.Add(new dim_teamMap());
            modelBuilder.Configurations.Add(new dim_time_zoneMap());
            modelBuilder.Configurations.Add(new dim_workloadMap());
            modelBuilder.Configurations.Add(new etl_jobMap());
            modelBuilder.Configurations.Add(new etl_job_delayedMap());
            modelBuilder.Configurations.Add(new etl_job_executionMap());
            modelBuilder.Configurations.Add(new etl_job_scheduleMap());
            modelBuilder.Configurations.Add(new etl_job_schedule_periodMap());
            modelBuilder.Configurations.Add(new etl_jobstepMap());
            modelBuilder.Configurations.Add(new etl_jobstep_errorMap());
            modelBuilder.Configurations.Add(new etl_jobstep_executionMap());
            modelBuilder.Configurations.Add(new etl_maintenance_configurationMap());
            modelBuilder.Configurations.Add(new fact_agentMap());
            modelBuilder.Configurations.Add(new fact_agent_queueMap());
            modelBuilder.Configurations.Add(new fact_forecast_workloadMap());
            modelBuilder.Configurations.Add(new fact_hourly_availabilityMap());
            modelBuilder.Configurations.Add(new fact_kpi_targets_teamMap());
            modelBuilder.Configurations.Add(new fact_qualityMap());
            modelBuilder.Configurations.Add(new fact_queueMap());
            modelBuilder.Configurations.Add(new fact_requestMap());
            modelBuilder.Configurations.Add(new fact_requested_daysMap());
            modelBuilder.Configurations.Add(new fact_scheduleMap());
            modelBuilder.Configurations.Add(new fact_schedule_day_countMap());
            modelBuilder.Configurations.Add(new fact_schedule_deviationMap());
            modelBuilder.Configurations.Add(new fact_schedule_forecast_skillMap());
            modelBuilder.Configurations.Add(new fact_schedule_preferenceMap());
            modelBuilder.Configurations.Add(new FolderMap());
            modelBuilder.Configurations.Add(new language_translationMap());
            modelBuilder.Configurations.Add(new period_typeMap());
            modelBuilder.Configurations.Add(new permission_reportMap());
            modelBuilder.Configurations.Add(new pm_userMap());
            modelBuilder.Configurations.Add(new reportMap());
            modelBuilder.Configurations.Add(new report_controlMap());
            modelBuilder.Configurations.Add(new report_control_collectionMap());
            modelBuilder.Configurations.Add(new report_user_settingMap());
            modelBuilder.Configurations.Add(new Report1Map());
            modelBuilder.Configurations.Add(new scorecard_kpiMap());
            modelBuilder.Configurations.Add(new service_level_calculationMap());
            modelBuilder.Configurations.Add(new sys_configurationMap());
            modelBuilder.Configurations.Add(new sys_crossdatabaseviewMap());
            modelBuilder.Configurations.Add(new sys_crossdatabaseview_targetMap());
            modelBuilder.Configurations.Add(new sys_datasourceMap());
            modelBuilder.Configurations.Add(new sys_etl_running_lockMap());
            modelBuilder.Configurations.Add(new MessageMap());
            modelBuilder.Configurations.Add(new MessagesPurgedMap());
            modelBuilder.Configurations.Add(new Queues1Map());
            modelBuilder.Configurations.Add(new SubscriptionStorageMap());
            modelBuilder.Configurations.Add(new ActualAgentStateMap());
            modelBuilder.Configurations.Add(new ExternalAgentStateMap());
            modelBuilder.Configurations.Add(new stg_absenceMap());
            modelBuilder.Configurations.Add(new stg_activityMap());
            modelBuilder.Configurations.Add(new stg_agent_skillMap());
            modelBuilder.Configurations.Add(new stg_agent_skillsetMap());
            modelBuilder.Configurations.Add(new stg_business_unitMap());
            modelBuilder.Configurations.Add(new stg_dateMap());
            modelBuilder.Configurations.Add(new stg_day_offMap());
            modelBuilder.Configurations.Add(new stg_forecast_workloadMap());
            modelBuilder.Configurations.Add(new stg_group_page_personMap());
            modelBuilder.Configurations.Add(new stg_hourly_availabilityMap());
            modelBuilder.Configurations.Add(new stg_kpiMap());
            modelBuilder.Configurations.Add(new stg_kpi_targets_teamMap());
            modelBuilder.Configurations.Add(new stg_overtimeMap());
            modelBuilder.Configurations.Add(new stg_permission_reportMap());
            modelBuilder.Configurations.Add(new stg_personMap());
            modelBuilder.Configurations.Add(new stg_queueMap());
            modelBuilder.Configurations.Add(new stg_queue_workloadMap());
            modelBuilder.Configurations.Add(new stg_requestMap());
            modelBuilder.Configurations.Add(new stg_scenarioMap());
            modelBuilder.Configurations.Add(new stg_scheduleMap());
            modelBuilder.Configurations.Add(new stg_schedule_changedMap());
            modelBuilder.Configurations.Add(new stg_schedule_day_absence_countMap());
            modelBuilder.Configurations.Add(new stg_schedule_day_off_countMap());
            modelBuilder.Configurations.Add(new stg_schedule_forecast_skillMap());
            modelBuilder.Configurations.Add(new stg_schedule_preferenceMap());
            modelBuilder.Configurations.Add(new stg_schedule_updated_personLocalMap());
            modelBuilder.Configurations.Add(new stg_schedule_updated_ShiftStartDateUTCMap());
            modelBuilder.Configurations.Add(new stg_scorecardMap());
            modelBuilder.Configurations.Add(new stg_scorecard_kpiMap());
            modelBuilder.Configurations.Add(new stg_shift_categoryMap());
            modelBuilder.Configurations.Add(new stg_skillMap());
            modelBuilder.Configurations.Add(new stg_time_zoneMap());
            modelBuilder.Configurations.Add(new stg_time_zone_bridgeMap());
            modelBuilder.Configurations.Add(new stg_userMap());
            modelBuilder.Configurations.Add(new stg_workloadMap());
            modelBuilder.Configurations.Add(new vw_aspnet_ApplicationsMap());
            modelBuilder.Configurations.Add(new vw_aspnet_MembershipUsersMap());
            modelBuilder.Configurations.Add(new vw_aspnet_UsersMap());
            modelBuilder.Configurations.Add(new v_agent_infoMap());
            modelBuilder.Configurations.Add(new v_agent_loggMap());
            modelBuilder.Configurations.Add(new v_bridge_acd_login_person_dateMap());
            modelBuilder.Configurations.Add(new v_ccc_system_infoMap());
            modelBuilder.Configurations.Add(new v_log_objectMap());
            modelBuilder.Configurations.Add(new v_permission_reportMap());
            modelBuilder.Configurations.Add(new v_queue_loggMap());
            modelBuilder.Configurations.Add(new v_queuesMap());
            modelBuilder.Configurations.Add(new v_reportMap());
            modelBuilder.Configurations.Add(new v_report_controlMap());
            modelBuilder.Configurations.Add(new v_report_control_collectionMap());
            modelBuilder.Configurations.Add(new v_stg_queueMap());
            modelBuilder.Configurations.Add(new v_time_zone_convertUpDownMap());
            modelBuilder.Configurations.Add(new v_stg_schedule_loadMap());
        }
    }
}
