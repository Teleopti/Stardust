using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IDataSetup
	{
		void Apply(ICurrentUnitOfWork currentUnitOfWork);
	}
}