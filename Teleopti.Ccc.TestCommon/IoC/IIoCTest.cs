namespace Teleopti.Ccc.TestCommon.IoC
{
	[IoCTest]
	public interface IIoCTest<T>
	{
		T Target { get; set; }
	}
}