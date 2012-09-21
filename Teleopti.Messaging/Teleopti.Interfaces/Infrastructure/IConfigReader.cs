using System.Collections.Specialized;

namespace Teleopti.Interfaces.Infrastructure
{
	/// <summary>
	/// A wrapper around ConfigurationManager
	/// </summary>
	public interface IConfigReader
	{
		/// <summary>
		/// Returns the AppSettings 
		/// </summary>
		NameValueCollection AppSettings { get; }
	}
}