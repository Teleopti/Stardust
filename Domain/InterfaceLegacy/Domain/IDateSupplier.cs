namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	/// <summary>
	/// Contains a date only that can be used.
	/// </summary>
	public interface IDateSupplier
	{
		///<summary>
		/// Gets the date.
		///</summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Date")]
		DateOnly Date { get; }
	}
}