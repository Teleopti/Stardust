using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Events
{
	///<summary>
	/// Message with details to perform an export of forecasting data from sub skills of multisite skills to a regular skill.
	///</summary>
	public class ExportMultisiteSkillsToSkillEvent : StardustJobInfo
	{
		///<summary>
		/// Creates a new instance of the <see cref="ExportMultisiteSkillsToSkillEvent"/>.
		///</summary>
		public ExportMultisiteSkillsToSkillEvent()
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
		public DateTime PeriodStart { get; set; }
		public DateTime PeriodEnd { get; set; }

		///<summary>
		/// The selection of data to export for multisite skills.
		///</summary>
		public ICollection<MultisiteSkillSelection> MultisiteSkillSelections { get; private set; }

		/// <summary>
		/// The identity of this message.
		/// </summary>
		public Guid Identity
		{
			get { return JobId; }
		}

		/// <summary>
		/// The owner of this action.
		/// </summary>
		public Guid OwnerPersonId { get; set; }
	}
}