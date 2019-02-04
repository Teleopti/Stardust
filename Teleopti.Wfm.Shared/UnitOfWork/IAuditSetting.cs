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