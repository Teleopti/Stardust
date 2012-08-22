using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IDataSetup
	{
		void Apply(IUnitOfWork uow);
	}
}