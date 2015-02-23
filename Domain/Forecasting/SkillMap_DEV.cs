

using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface ISkillMap_DEV : IAggregateRoot
	{
		ISkill MappedSkill { get; set; }
		ISkill Parent { get; }
	}

	public class SkillMap_DEV : SimpleAggregateRoot, ISkillMap_DEV
	{
		private readonly ISkill _parent;
		private ISkill _mappedSkill;

		protected SkillMap_DEV()
		{
		}

		public SkillMap_DEV(ISkill parent, ISkill mappedSkill)
		{
			_parent = parent;
			_mappedSkill = mappedSkill;
		}

		public virtual ISkill MappedSkill
		{
			get { return _mappedSkill; }
			set { _mappedSkill = value; }
		}

		public virtual ISkill Parent
		{
			get { return _parent; }
		}
	}
}