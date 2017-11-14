namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface IIoCTestContext
	{
		void SimulateShutdown();
		void SimulateRestart();
		void SimulateNewRequest();
	}
}