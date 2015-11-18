using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class AuditSetting : IAuditSetting
	{
		protected virtual int Id { get; set; }

		public virtual bool IsScheduleEnabled { get; protected set; }

		public virtual void TurnOffScheduleAuditing(IAuditSetter auditSettingSetter)
		{
			IsScheduleEnabled = false;
			auditSettingSetter.SetEntity(this);
		}

		public virtual void TurnOnScheduleAuditing(IAuditSettingRepository auditSettingRepository, 
															IAuditSetter auditSettingSetter)
		{
			auditSettingRepository.TruncateAndMoveScheduleFromCurrentToAuditTables();
			IsScheduleEnabled = true;
			auditSettingSetter.SetEntity(this);
		}

		public virtual bool ShouldBeAudited(object entity)
		{
			if (IsScheduleEnabled == false)
				return false;

			var root = aggregateRootFromEntity(entity);
			var schedData = root as IPersistableScheduleData;
			if (schedData!=null)
			{
				var scenario = schedData.Scenario;
				if (scenario != null && scenario.DefaultScenario)
					return true;
			}

			return false;
		}

		private static IAggregateRoot aggregateRootFromEntity(object entity)
		{
			var aggRoot = entity as IAggregateRoot;
			if (aggRoot == null)
			{
				var aggEntity = entity as IAggregateEntity;
				if (aggEntity != null)
				{
					aggRoot = aggEntity.Root();
				}
			}
			return aggRoot;
		}
	}
}