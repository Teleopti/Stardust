using System;

namespace Teleopti.Interfaces.Domain
{

	/// <summary>
	/// 
	/// </summary>
	public interface ISimpleTimeZone
	{
		/// <summary>
		/// 
		/// </summary>
		Int16 Id { get; set; }
		/// <summary>
		/// 
		/// </summary>
		string Name { get; set; }
		/// <summary>
		/// 
		/// </summary>
		int Distance { get; set; }

	}
}