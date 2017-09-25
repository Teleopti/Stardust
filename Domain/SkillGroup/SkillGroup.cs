using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Intraday;

namespace Teleopti.Ccc.Domain.SkillGroup
{
	public class SkillGroup : VersionedAggregateRootWithBusinessUnit, IDeleteTag
	{
		private ICollection<SkillInIntraday> _skills;
		private string _name;
		private bool _isDeleted;

		public virtual ICollection<SkillInIntraday> Skills
		{
			get { return _skills; }
			set { _skills = value; }
		}

		public virtual string Name
		{
			get { return _name; }
			set { _name = value; }
		}

		public virtual bool IsDeleted
		{
			get { return _isDeleted; }
		}

		public virtual void SetDeleted()
		{
			_isDeleted = true;
		}
	}
}
