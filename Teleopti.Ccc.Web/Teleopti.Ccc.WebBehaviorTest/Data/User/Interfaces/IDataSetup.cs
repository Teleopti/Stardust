using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces
{
	public interface IDataSetup
	{
		void Apply(IUnitOfWork uow);
	}
}