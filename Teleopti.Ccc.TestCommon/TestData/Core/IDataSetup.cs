using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IDataSetup
	{
		void Apply(ICurrentUnitOfWork currentUnitOfWork);
	}
}