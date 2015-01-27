using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.Security.AuthorizationEntities
{
	public class AllFunctions
	{
		public AllFunctions(ICollection<SystemFunction> functions)
		{
			Functions = functions;
		}

		public ICollection<SystemFunction> Functions { get; private set; }

		public SystemFunction FindByFunctionPath(string functionPath)
		{
			return Functions.Select(systemFunction => systemFunction.FindByPath(functionPath)).FirstOrDefault(foundFunction => foundFunction != null);
		}

		public SystemFunction FindByForeignId(string foreignId)
		{
			return Functions.Select(systemFunction => systemFunction.FindByForeignId(foreignId)).FirstOrDefault(foundFunction => foundFunction != null);
		}
	}
}