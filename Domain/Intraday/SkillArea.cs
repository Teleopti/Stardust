using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Intraday
{
	public class SkillArea : VersionedAggregateRootWithBusinessUnit, IDeleteTag
	{
		private ICollection<SkillInIntraday> _skillCollection;
		private string _name;
		private bool _isDeleted;

		public virtual ICollection<SkillInIntraday> SkillCollection
		{
			get { return _skillCollection; }
			set { _skillCollection = value; }
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
