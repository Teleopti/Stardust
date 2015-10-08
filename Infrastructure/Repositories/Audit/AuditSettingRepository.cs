using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class AuditSettingRepository : IAuditSettingRepository
	{
		private readonly ICurrentUnitOfWork _currentUnitOfWork;

		public const string MissingAuditSetting = "Missing audit setting in database!";

		public AuditSettingRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			_currentUnitOfWork = currentUnitOfWork;
		}

		public void TruncateAndMoveScheduleFromCurrentToAuditTables()
		{
			const int fiveMinutes = 5 * 60;
			const string sql = "exec Auditing.InitAuditTables";
			_currentUnitOfWork.Session().CreateSQLQuery(sql)
				.SetTimeout(fiveMinutes)
				.ExecuteUpdate();
		}

		public IAuditSetting Read()
		{
			var auditSetting = _currentUnitOfWork.Session().Get<AuditSetting>(AuditSetting.TheId);
			if(auditSetting==null)
				throw new DataSourceException(MissingAuditSetting);
			return auditSetting;
		}
	}
}