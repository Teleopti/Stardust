namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Audit setting
	/// </summary>
	public interface IAuditSetting
	{
		/// <summary>
		/// Is schedule data audited
		/// </summary>
		bool IsScheduleEnabled { get; }

		/// <summary>
		/// turns off schedule auditing
		/// </summary>
		void TurnOffScheduleAuditing(IAuditSetter auditSettingSetter);

//		/// <summary>
//		/// turns on schedule auditing
//		/// </summary>
//		/// <param name="auditSettingRepository"></param>
//		/// <param name="auditSettingSetter">A callback to audit system</param>
//		void TurnOnScheduleAuditing(IAuditSettingRepository auditSettingRepository, IAuditSetter auditSettingSetter);

		/// <summary>
		/// determines wheter entity should be audited or not
		/// </summary>
		/// <param name="entity"></param>
		/// <returns></returns>
		bool ShouldBeAudited(object entity);
	}

	public static class AuditSettingDefault
	{
		public const int TheId = 1;
	}
}