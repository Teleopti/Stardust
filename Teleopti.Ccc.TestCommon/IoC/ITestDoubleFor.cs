namespace Teleopti.Ccc.TestCommon.IoC
{
	public interface ITestDoubleFor
	{
		void For<T>();
		void For<T1, T2>();
		void For<T1, T2, T3>();
		void For<T1, T2, T3, T4>();
	}
}