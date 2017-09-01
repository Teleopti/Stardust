using System;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillInIntraday
	{
		private string _name;
		private Guid _id;
		private bool _isDeleted;
		private bool _doDisplayData;
		private string _skillType;
		private bool _isMultisiteSkill;
		private bool _showAbandonRate;
		private bool _showReforecastedAgents;

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual Guid Id
		{
			get { return _id; }
			set { _id = value; }
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
			set { _isDeleted = value; }
		}

		public virtual bool DoDisplayData
		{
			get { return _doDisplayData; }
			set { _doDisplayData = value; }
		}

		public virtual string SkillType
		{
			get { return _skillType; }
			set { _skillType = value; }
		}

		public virtual bool IsMultisiteSkill
		{
			get { return _isMultisiteSkill; }
			set { _isMultisiteSkill = value; }
		}

		public virtual bool ShowAbandonRate
		{
			get { return _showAbandonRate; }
			set { _showAbandonRate = value; }
		}

		public virtual bool ShowReforecastedAgents
		{
			get { return _showReforecastedAgents; }
			set { _showReforecastedAgents = value; }
		}
	}
}