using NHibernate;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AuditSettingProvider
	{
		private volatile AuditSetting _entity;
		private readonly object locker = new object();

		public AuditSetting Entity(ISession session)
		{
			if (_entity == null)
			{
				lock (locker)
				{
					if (_entity == null)
					{
						var auditSetting = session.Get<AuditSetting>(AuditSetting.TheId);
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