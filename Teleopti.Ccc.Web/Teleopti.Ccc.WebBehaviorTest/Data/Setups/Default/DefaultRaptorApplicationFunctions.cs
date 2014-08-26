using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.Setups.Default
{
	public class DefaultRaptorApplicationFunctions : IHashableDataSetup
	{
		private readonly DefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory = 
			new DefinedRaptorApplicationFunctionFactory();

		public void Apply(IUnitOfWork uow)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(uow);

			applicationFunctionRepository.AddRange(definedRaptorApplicationFunctionFactory.ApplicationFunctionList);
		}

		public int HashValue()
		{
			return definedRaptorApplicationFunctionFactory.ApplicationFunctionList
				.Aggregate(123, (current, applicationFunction) => current ^ applicationFunction.FunctionCode.GetHashCode());
		}
	}
}