using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Configuration
{

    /// <summary>
    /// This will load setting pages from external module.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 1/19/2009
    /// </remarks>
    public interface IExternalModuleLoader
    {

        /// <summary>
        /// Loads settings from external module.
        /// </summary>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        void LoadFromExternalModule(SelectedEntity<IAggregateRoot> entity);

        /// <summary>
        /// Gets the type of the view.
        /// </summary>
        /// <value>The type of the view.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 1/19/2009
        /// </remarks>
        ViewType ViewType { get; }

    }

    public enum ViewType
    {
        KpiSettings,
        ScorecardSettings,
        Availability,
        Contract,
        PartTimePercentage,
        ContractSchedule,
        Activity,
        MasterActivity,
        Absence,
        Scenario,
        CommonAgentNameDescription,
        Rotation,
        OptionalColumns,
        TeamSchedulePublishing,
        OrganizationTree,
		Sites,
        StateGroup,
        ShiftCategorySettings,
        SystemSetting,
        DaysOff,
        Alarm,
        ManageAlarmSituations,
        JusticeValues,
        Multiplicator, 
        MultiplicatorDefinitionSets,
        WorkflowControlSets,
        SetScorecard,
        ChangeYourPassword,
        AuditTrailSetting, 
        ScheduleTag,
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
		SmsSettings,
		Gamification,
		Seniority
    } ;
}
