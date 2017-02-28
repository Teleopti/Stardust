namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Interface for text formatter classes
	/// </summary>
	public interface ITextFormatter
	{
		/// <summary>
		/// Formats the text
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
		string Format(string value);
	}
}