using System;
using System.Runtime.CompilerServices;
using NHibernate;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class AuditSetter : IAuditSetter
	{
		private readonly Func<ISession, IAuditSetting> _entityDel;
		private IAuditSetting _entity;

		public AuditSetter(Func<ISession, IAuditSetting> entityDel)
		{
			InParameter.NotNull("entityDel", entityDel);
			_entityDel = entityDel;
		}

		public IAuditSetting Entity(ISession session)
		{
			if (_entity == null)
			{
				SetEntity(_entityDel(session));
			}
			return _entity;
		}

		[MethodImplAttribute(MethodImplOptions.Synchronized)]
		public void SetEntity(IAuditSetting entity)
		{
			_entity = entity;
		}
	}
}