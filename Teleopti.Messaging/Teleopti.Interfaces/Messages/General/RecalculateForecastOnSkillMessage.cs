using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Messages.General
{
	///<summary>
	/// Message with details to perform an recalc of forecasting data on a skill.
	///</summary>
	public class RecalculateForecastOnSkillMessage : RaptorDomainMessage
	{
		private readonly Guid _messageId = Guid.NewGuid();

		/// <summary>
		/// Gets the message identity.
		/// </summary>
		public override Guid Identity
		{
			get { return _messageId; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="skill"></param>
		public RecalculateForecastOnSkillMessage(ISkill skill)
		{
			Skill = skill;
		}

		///<summary>
		/// The period to recalculate in the skills time zone.
		///</summary>
		public DateOnlyPeriod Period { get; set; }

		///<summary>
		/// The skill to recalculate.
		///</summary>
		public ISkill Skill { get; set; }

		/// <summary>
		/// The owner of this action. can we use this to say that this person updated the forecast????
		/// </summary>
		public Guid OwnerPersonId { get; set; }


		// add information how the forecast should change
		// percent up/down or some other way
	}
}