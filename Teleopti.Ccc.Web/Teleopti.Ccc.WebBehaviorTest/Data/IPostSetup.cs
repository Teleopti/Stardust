using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebBehaviorTest.Data
{
	public interface IPostSetup
	{
		void Apply(IPerson user);
	}
}