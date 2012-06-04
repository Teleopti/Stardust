using Teleopti.Ccc.WebBehaviorTest.Data.User.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data.User
{
	public interface IContractSetup : IDataSetup
	{
		IContract Contract { get; }
	}
}