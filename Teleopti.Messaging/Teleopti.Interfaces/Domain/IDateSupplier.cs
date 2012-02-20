namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Contains a date only that can be used.
	/// </summary>
	public interface IDateSupplier
	{
		///<summary>
		/// Gets the date.
		///</summary>
		DateOnly Date { get; }
	}
}