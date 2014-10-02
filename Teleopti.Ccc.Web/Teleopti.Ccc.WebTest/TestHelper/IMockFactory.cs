namespace Teleopti.Ccc.WebTest.TestHelper
{
	public interface IMockFactory 
	{
		IMockProxy<T> DynamicMock<T>() where T : class;
	}
}