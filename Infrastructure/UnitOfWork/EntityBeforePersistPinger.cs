using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
	public class EntityBeforePersistPinger
	{
		public void Execute(IEnumerable<object> entities)
		{
			foreach (var entity in entities.ToArray())
			{
				if (entity is IBeforePersist persist)
				{
					persist.Ping();					
				}
			}
		}
	}
}