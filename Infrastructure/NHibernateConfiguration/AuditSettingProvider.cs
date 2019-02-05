using NHibernate;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AuditSettingProvider
	{
		private volatile IAuditSetting _entity;
		private readonly object locker = new object();

		public IAuditSetting Entity(ISession session)
		{
			if (_entity == null)
			{
				lock (locker)
				{
					if (_entity == null)
					{
						var auditSetting = session.Get<AuditSetting>(AuditSettingDefault.TheId);
						if (auditSetting == null)
							throw new DataSourceException(AuditSettingRepository.MissingAuditSetting);
						_entity = auditSetting;
					}
				}
			}
			return _entity;
		}
	}
}