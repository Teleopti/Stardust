using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;

namespace Teleopti.Ccc.Domain.Auditing
{
	public class PurgeAuditRunner
	{
		private readonly IResolve _resolve;

		public PurgeAuditRunner(IResolve resolve)
		{
			_resolve = resolve;
		}

		public void Run()
		{
			var types = _resolve.ConcreteTypesFor(typeof(IPurgeAudit));

			foreach (var type in types)
			{
				var purge = _resolve.Resolve(type) as IPurgeAudit;
				purge.PurgeAudits();
			}
			
			//TODO: make sure to handle exceptions in IPurgeAudit implementations
		}
	}
}
