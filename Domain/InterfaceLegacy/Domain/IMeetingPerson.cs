using System;

namespace Teleopti.Interfaces.Domain
{
	/// <summary>
	/// Person stored in meetings
	/// </summary>
	public interface IMeetingPerson : IAggregateEntity, ICloneableEntity<IMeetingPerson>
	{
		/// <summary>
		/// Get/Set person
		/// </summary>
		IPerson Person { get; set; }

		/// <summary>
		/// Get/Set optional value
		/// </summary>
		//rk suppressar denna nu då jag är mitt inne i en stor upgradering och inte orkar med dbtrunkscript
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Optional")]
		Boolean Optional { get; set; }
	}
}