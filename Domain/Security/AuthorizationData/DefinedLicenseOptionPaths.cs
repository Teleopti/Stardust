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
		public const string TeleoptiWfmOvertimeRequests = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/OvertimeRequests";
		public const string TeleoptiWfmGrant = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/Grant";
		public const string TeleoptiWfmInsights = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/Insights";

		public const string TeleoptiWfmVNextPilot = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/VNext";
		public const string TeleoptiWfmOutbound = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/Outbound";
		public const string TeleoptiWfmSeatPlanner = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/SeatPlanner";

        /// <summary>
        /// Constant string for Freemium base option
        /// </summary>
        public const string TeleoptiCccFreemiumForecasts = DefinedLicenseSchemaCodes.TeleoptiWFMForecastsSchema+ "/Forecasts";
        /// <summary>
        /// Constant string for Early bird base option
        /// </summary>
        public const string TeleoptiCccPilotCustomersBpoExchange = DefinedLicenseSchemaCodes.TeleoptiWFMSchema + "/BpoExchange";
	}
}
