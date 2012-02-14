namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// repository for audit setting
	/// </summary>
	public interface IAuditSettingRepository
	{
		/// <summary>
		/// Truncates and move schedule data from live tables to audit tables.
		/// Only schedules in default scenario will be audited!
		/// </summary>
		void TruncateAndMoveScheduleFromCurrentToAuditTables();

		/// <summary>
		/// Gets the audit setting
		/// </summary>
		/// <returns></returns>
		IAuditSetting Read();
	}
}