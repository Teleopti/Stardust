using MvcContrib.TestHelper.MockFactories;
using Rhino.Mocks;

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