namespace Teleopti.Ccc.WebBehaviorTest.Core.BrowserImpl
{
	public interface IBrowserHandler<T>
	{
		T Start();
		void PrepareForTestRun();
		void Close();
		T Restart();
	}
}