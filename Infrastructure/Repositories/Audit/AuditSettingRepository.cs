using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Audit
{
	public class AuditSettingRepository : Repository, IAuditSettingRepository
	{
		public const string MissingAuditSetting = "Missing audit setting in database!";

		public AuditSettingRepository(IUnitOfWork unitOfWork)
			:base(unitOfWork)
		{
		}

		public AuditSettingRepository(IUnitOfWorkFactory unitOfWorkFactory)
			:base(unitOfWorkFactory)
		{
		}

		public void TruncateAndMoveScheduleFromCurrentToAuditTables()
		{
			const string sql = "exec Auditing.InitAuditTables";
			Session.CreateSQLQuery(sql)
					.ExecuteUpdate();
		}

		public IAuditSetting Read()
		{
			var auditSetting = Session.Get<AuditSetting>(AuditSetting.TheId);
			if(auditSetting==null)
				throw new DataSourceException(MissingAuditSetting);
			return auditSetting;
		}
	}
}