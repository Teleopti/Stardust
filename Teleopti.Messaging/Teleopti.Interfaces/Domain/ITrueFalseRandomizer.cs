namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for TruEFalseRandomizer
	/// </summary>
	public interface ITrueFalseRandomizer
	{
		/// <summary>
		/// Randomizes the specified seed.
		/// </summary>
		/// <param name="seed">The seed.</param>
		/// <returns></returns>
		bool Randomize(int seed);
	}
}