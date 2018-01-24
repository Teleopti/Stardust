using System.Collections.Generic;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EntityPinger
	{
		public void Execute(IEnumerable<object> entities)
		{
			foreach (var entity in entities)
			{
				if (entity is IBeforePersist persist)
				{
					persist.Ping();					
				}
			}
		}
	}
}