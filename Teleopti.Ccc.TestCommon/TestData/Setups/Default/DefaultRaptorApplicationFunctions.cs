using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Setups.Default
{
	public class DefaultRaptorApplicationFunctions : IHashableDataSetup
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(DefaultRaptorApplicationFunctions));

		private readonly DefinedRaptorApplicationFunctionFactory definedRaptorApplicationFunctionFactory = 
			new DefinedRaptorApplicationFunctionFactory();

		public void Apply(ICurrentUnitOfWork currentUnitOfWork)
		{
			var applicationFunctionRepository = new ApplicationFunctionRepository(currentUnitOfWork);

			applicationFunctionRepository.AddRange(definedRaptorApplicationFunctionFactory.ApplicationFunctionList);
		}

		public int HashValue()
		{
			var hashValue = definedRaptorApplicationFunctionFactory.ApplicationFunctionList
				.Aggregate(123, (current, applicationFunction) => current ^ applicationFunction.FunctionCode.GetHashCode());
			log.Debug("hashValue " + hashValue);
			return hashValue;
		}
	}
}