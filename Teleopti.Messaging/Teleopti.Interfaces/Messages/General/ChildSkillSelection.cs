using System;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Mapping of export skill combination.
	///</summary>
	public class ChildSkillSelection
	{
		/// <summary>
		/// The id of the source skill (export from).
		/// </summary>
		public Guid SourceSkillId { get; set; }

		///<summary>
		/// The id of the target skill (import to).
		///</summary>
		public Guid TargetSkillId { get; set; }
	}
}