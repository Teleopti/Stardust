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
        public const string TeleoptiCccBase = DefinedLicenseSchemaCodes.TeleoptiWFMSchema +  "/Base";
        /// <summary>
        /// Constant string for Raptor ASS option
        /// </summary>
		public const string TeleoptiCccLifestyle = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/Lifestyle";
        public const string TeleoptiCccShiftTrader = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/ShiftTrader";
		public const string TeleoptiCccVacationPlanner = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/VacationPlanner";
		public const string TeleoptiCccOvertimeAvailability = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/OvertimeAvailability";

        public const string TeleoptiCccAgentScheduleMessenger = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/AgentScheduleMessenger";
        public const string TeleoptiCccRealTimeAdherence = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/RealTimeAdherence";
        public const string TeleoptiCccPerformanceManager = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/PerformanceManager";
        public const string TeleoptiCccPayrollIntegration = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/PayrollIntegration";
		public const string TeleoptiCccSmsLink = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/SMSLink";
		public const string TeleoptiCccCalendarLink = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/CalendarLink";

		public const string TeleoptiCccNotify = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/Notify";

		public const string TeleoptiCccMyTeam = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/MyTeam";


        /// <summary>
        /// Constant string for Freemium base option
        /// </summary>
        public const string TeleoptiCccFreemiumForecasts = DefinedLicenseSchemaCodes.TeleoptiWFMForecastsSchema+ "/Forecasts";
        /// <summary>
        /// Constant string for Early bird base option
        /// </summary>
        public const string TeleoptiCccPilotCustomersBase = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Base";
        public const string TeleoptiCccPilotCustomersForecasts = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Forecasts";
        public const string TeleoptiCccPilotCustomersShifts = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Shifts";
        public const string TeleoptiCccPilotCustomersPeople = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/People";
        public const string TeleoptiCccPilotCustomersAgentPortal = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/AgentPortal";
        public const string TeleoptiCccPilotCustomersOptions = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Options";
        public const string TeleoptiCccPilotCustomersScheduler = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Scheduler";
        public const string TeleoptiCccPilotCustomersIntraday = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Intraday";
        public const string TeleoptiCccPilotCustomersPermissions = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Permissions";
        public const string TeleoptiCccPilotCustomersReports = DefinedLicenseSchemaCodes.TeleoptiWFMPilotCustomersSchema + "/Reports";
    }
}
