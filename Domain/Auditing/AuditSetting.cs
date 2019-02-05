using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class AuditSetting
	{
		protected virtual int Id { get; set; }

		public virtual bool IsScheduleEnabled { get; protected set; }


		public virtual bool ShouldBeAudited(object entity)
		{
			if (IsScheduleEnabled == false)
				return false;

			var root = aggregateRootFromEntity(entity);
			if (root is IPersistableScheduleData schedData)
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
				if (entity is IAggregateEntity aggEntity)
				{
					aggRoot = aggEntity.Root();
				}
			}
			return aggRoot;
		}
	}
}