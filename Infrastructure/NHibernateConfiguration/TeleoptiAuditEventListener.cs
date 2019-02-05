using NHibernate;
using NHibernate.Envers.Event;
using NHibernate.Event;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class TeleoptiAuditEventListener : AuditEventListener
	{
		private readonly AuditSettingProvider _auditSetting;

		public TeleoptiAuditEventListener(AuditSettingProvider auditSetting)
		{
			_auditSetting = auditSetting;
		}

		public override void OnPostDelete(PostDeleteEvent evt)
		{
			if (auditEntity(evt.Entity, evt.Session))
				base.OnPostDelete(evt);
		}
		public override void OnPostInsert(PostInsertEvent evt)
		{
			if (auditEntity(evt.Entity, evt.Session))
				base.OnPostInsert(evt);
		}
		public override void OnPostRecreateCollection(PostCollectionRecreateEvent evt)
		{
			if (auditEntity(evt.AffectedOwnerOrNull, evt.Session))
				base.OnPostRecreateCollection(evt);
		}
		public override void OnPostUpdate(PostUpdateEvent evt)
		{
			if (auditEntity(evt.Entity, evt.Session))
				base.OnPostUpdate(evt);
		}
		public override void OnPreRemoveCollection(PreCollectionRemoveEvent evt)
		{
			if (auditEntity(evt.AffectedOwnerOrNull, evt.Session))
				base.OnPreRemoveCollection(evt);
		}
		public override void OnPreUpdateCollection(PreCollectionUpdateEvent evt)
		{
			if (auditEntity(evt.AffectedOwnerOrNull, evt.Session))
				base.OnPreUpdateCollection(evt);
		}

		private bool auditEntity(object entity, ISession session)
		{
			return _auditSetting.Entity(session).ShouldBeAudited(entity);
		}
	}
}