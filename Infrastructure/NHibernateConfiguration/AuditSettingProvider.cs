using System;
using System.Runtime.CompilerServices;
using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AuditSettingProvider
	{
		private readonly Func<ISession, IAuditSetting> _entityDel;
		private volatile IAuditSetting _entity;
		private readonly object locker = new object();

		public AuditSettingProvider(Func<ISession, IAuditSetting> entityDel)
		{
			InParameter.NotNull(nameof(entityDel), entityDel);
			_entityDel = entityDel;
		}

		public IAuditSetting Entity(ISession session)
		{
			if (_entity == null)
			{
				lock (locker)
				{
					if (_entity == null)
					{
						_entity = _entityDel(session);
					}
				}
			}
			return _entity;
		}
	}
}