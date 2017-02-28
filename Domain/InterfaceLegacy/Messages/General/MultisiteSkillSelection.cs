using System;
using System.Collections.Generic;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// The selection of data to export for one multisite skill.
	///</summary>
	public class MultisiteSkillSelection
	{
		/// <summary>
		/// Creates a new instance of the <see cref="MultisiteSkillSelection"/>.
		/// </summary>
		public MultisiteSkillSelection()
		{
			ChildSkillSelections = new List<ChildSkillSelection>();
		}

		///<summary>
		/// The Id of the multisite skill to export.
		///</summary>
		public Guid MultisiteSkillId { get; set; }

		///<summary>
		/// The selection of data to export for child skills belonging to this multisite skill.
		///</summary>
		public ICollection<ChildSkillSelection> ChildSkillSelections { get; private set; }
	}
}