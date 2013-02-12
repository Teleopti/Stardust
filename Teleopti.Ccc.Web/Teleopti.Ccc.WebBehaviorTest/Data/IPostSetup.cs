using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IPostSetup
	{
		void Apply(IPerson user,IUnitOfWork uow);
	}
}