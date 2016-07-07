﻿using System;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Message with details to perform an export of forecasting data from sub skills of multisite skills to a regular skill.
	///</summary>
	public class ExportMultisiteSkillToSkill : MessageWithLogOnContext
	{
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
		public MultisiteSkillSelection MultisiteSkillSelections { get; set; }

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
