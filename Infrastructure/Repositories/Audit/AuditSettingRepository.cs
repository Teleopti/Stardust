using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

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

		public AuditSetting Read()
		{
			var auditSetting = _currentUnitOfWork.Session().Get<AuditSetting>(AuditSetting.TheId);
			if(auditSetting==null)
				throw new DataSourceException(MissingAuditSetting);
			return auditSetting;
		}
	}
}