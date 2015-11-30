using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Infrastructure.Toggle;

namespace Teleopti.Ccc.WebTest.Areas.Permissions
{
	public class FakeApplicationFunctionsToggleFilter : IApplicationFunctionsToggleFilter
	{
		private readonly AllFunctions _allFunction = new AllFunctions(new Collection<SystemFunction>());

		public AllFunctions FilteredFunctions()
		{
			return _allFunction;
		}


		public void AddFakeFunction(ApplicationFunction functionOne, Func<object, bool> definition)
		{
			_allFunction.Functions.Add(new SystemFunction(functionOne, definition));
		}

		public void AddFakeSystemFunction(SystemFunction systemFunction)
		{
			_allFunction.Functions.Add(systemFunction);
		}
	}
}