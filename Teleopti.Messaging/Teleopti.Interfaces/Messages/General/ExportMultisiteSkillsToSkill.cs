using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Message with details to perform an export of forecasting data from sub skills of multisite skills to a regular skill.
	///</summary>
	public class ExportMultisiteSkillsToSkill : MessageWithLogOnContext
	{
		///<summary>
		/// Creates a new instance of the <see cref="ExportMultisiteSkillsToSkill"/>.
		///</summary>
		public ExportMultisiteSkillsToSkill()
		{
			MultisiteSkillSelections = new List<MultisiteSkillSelection>();
		}

		///<summary>
		/// The Id of the job this message will feed with updates.
		///</summary>
		public Guid JobId { get; set; }

		///<summary>
		/// The period to export in the source skills time zone.
		///</summary>
		public DateOnlyPeriod Period { get; set; }

		///<summary>
		/// The selection of data to export for multisite skills.
		///</summary>
		public ICollection<MultisiteSkillSelection> MultisiteSkillSelections { get; private set; }

		/// <summary>
		/// The identity of this message.
		/// </summary>
		public override Guid Identity
		{
			get { return JobId; }
		}

		/// <summary>
		/// The owner of this action.
		/// </summary>
		public Guid OwnerPersonId { get; set; }
	}
}