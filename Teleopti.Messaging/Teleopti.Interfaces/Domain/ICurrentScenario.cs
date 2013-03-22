namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Give me the current scenario please! I dont care how =) 
	/// </summary>
	public interface ICurrentScenario
	{
		/// <summary>
		/// The scenarios current
		/// </summary>
		/// <returns></returns>
		IScenario Current();
	}
}