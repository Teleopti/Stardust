using Rhino.Mocks;
using Teleopti.Ccc.WebTest.TestHelper;

namespace Teleopti.Ccc.WebTest
{
	public class RhinoMocksStubFactory : IMockFactory
	{
		public IMockProxy<T> DynamicMock<T>() where T : class
		{
			return new RhinoMocksStubProxy<T>(MockRepository.GenerateMock<T>());
		}
	}
}