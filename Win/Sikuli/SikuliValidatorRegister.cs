namespace Teleopti.Ccc.Win.Sikuli
{
	internal static class SikuliValidatorRegister
	{

		/// <summary>
		/// Identifier for validators
		/// </summary>
		
		/// selftest validators
		public const string TestPass = "TestPass";
		public const string TestWarn = "TestWarn";
		public const string TestFail = "TestFail";
		public const string TestRoot = "TestRoot";

		/// real validators
		public const string None = "None";
		public const string DeleteAll = "DeleteAll";
		public const string Schedule = "Schedule";
		public const string ScheduleAllOptionsOff = "ScheduleAllOptionsOff"; 
		public const string ScheduleOvertimePeriod = "ScheduleOvertimePeriod";
		public const string Optimize = "Optimize";
		public const string OptimizeDaysOff = "OptimizeDaysOff";
		public const string OptimizeBetweenDays = "OptimizeBetweenDays";
		public const string OptimizeWithinDays = "OptimizeWithinDays";
		public const string OptimizeBlockTeam = "OptimizeBlockTeam";
		public const string OptimizeIntervalBalanceBefore = "OptimizeIntervalBalanceBefore";
		public const string OptimizeIntervalBalanceAfter = "OptimizeIntervalBalanceAfter";
		public const string ScheduleBlockSameShiftCategory = "ScheduleBlockSameShiftCategory";
		public const string OptimizeBlockSameShiftCategory = "OptimizeBlockSameShiftCategory";
		public const string ScheduleBlockSameStartTime = "ScheduleBlockSameStartTime";
		public const string OptimizeBlockSameStartTime = "OptimizeBlockSameStartTime";
	}
}
