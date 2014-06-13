namespace Teleopti.Ccc.Domain.Security.AuthorizationData
{
    /// <summary>
    /// License option path constants
    /// </summary>
    public static class DefinedLicenseOptionPaths
    {
        /// <summary>
        /// Constant string for Raptor base option
        /// </summary>
        public const string TeleoptiCccBase = DefinedLicenseSchemaCodes.TeleoptiCccSchema +  "/Base";
        /// <summary>
        /// Constant string for Developer license option
        /// </summary>
        public const string TeleoptiCccDeveloper = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/Developer";
        /// <summary>
        /// Constant string for Raptor ASS option
        /// </summary>
        public const string TeleoptiCccAgentSelfService = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/AgentSelfService";
        public const string TeleoptiCccShiftTrades = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/ShiftTrades";
        public const string TeleoptiCccAgentScheduleMessenger = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/AgentScheduleMessenger";
        public const string TeleoptiCccHolidayPlanner = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/HolidayPlanner";
        public const string TeleoptiCccRealTimeAdherence = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/RealTimeAdherence";
        public const string TeleoptiCccPerformanceManager = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/PerformanceManager";
        public const string TeleoptiCccPayrollIntegration = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/PayrollIntegration";
        public const string TeleoptiCccMyTimeWeb = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/MyTimeWeb";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Sms")]
		public const string TeleoptiCccSmsLink = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/SMSLink";
		public const string TeleoptiCccCalendarLink = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/CalendarLink";
        /// <summary>
        /// Constant string for Freemium base option
        /// </summary>
        public const string TeleoptiCccFreemiumForecasts = DefinedLicenseSchemaCodes.TeleoptiCccForecastsSchema+ "/Forecasts";
        /// <summary>
        /// Constant string for Early bird base option
        /// </summary>
        public const string TeleoptiCccPilotCustomersBase = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Base";
        public const string TeleoptiCccPilotCustomersForecasts = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Forecasts";
        public const string TeleoptiCccPilotCustomersShifts = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Shifts";
        public const string TeleoptiCccPilotCustomersPeople = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/People";
        public const string TeleoptiCccPilotCustomersAgentPortal = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/AgentPortal";
        public const string TeleoptiCccPilotCustomersOptions = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Options";
        public const string TeleoptiCccPilotCustomersScheduler = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Scheduler";
        public const string TeleoptiCccPilotCustomersIntraday = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Intraday";
        public const string TeleoptiCccPilotCustomersPermissions = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Permissions";
        public const string TeleoptiCccPilotCustomersReports = DefinedLicenseSchemaCodes.TeleoptiCccPilotCustomersSchema + "/Reports";
	
		public const string TeleoptiCccVersion8 = DefinedLicenseSchemaCodes.TeleoptiCccSchema + "/Version8";
	}
}
