using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;

namespace Teleopti.Ccc.TestCommon.TestData.Core
{
	public interface IDataSetup<T>
	{
		void Apply(T spec);
	}
	
	public interface IDataSetup
	{
		void Apply(ICurrentUnitOfWork currentUnitOfWork);
	}
}