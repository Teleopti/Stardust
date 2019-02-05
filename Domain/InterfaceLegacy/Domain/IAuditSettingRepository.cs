using Teleopti.Ccc.Domain.Auditing;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// repository for audit setting
	/// </summary>
	public interface IAuditSettingRepository
	{
		/// <summary>
		/// Gets the audit setting
		/// </summary>
		/// <returns></returns>
		AuditSetting Read();
	}
}