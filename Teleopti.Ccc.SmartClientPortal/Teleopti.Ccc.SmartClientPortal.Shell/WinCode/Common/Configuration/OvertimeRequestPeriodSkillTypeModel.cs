using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class OvertimeRequestPeriodSkillTypeModel
	{
		private readonly ISkillType _skillType;

		public OvertimeRequestPeriodSkillTypeModel(ISkillType skillType, string displayText)
		{
			_skillType = skillType;

			DisplayText = displayText;
		}

		public Guid Item => _skillType.Id.GetValueOrDefault();

		public ISkillType SkillType => _skillType;

		public string DisplayText { get; internal set; }

		public override bool Equals(object obj)
		{
			OvertimeRequestPeriodSkillTypeModel model = obj as OvertimeRequestPeriodSkillTypeModel;
			if (model == null) return false;

			return model.Item.Equals(_skillType.Id.GetValueOrDefault());
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((_skillType != null ? _skillType.GetHashCode() : 0) * 397) ^
					   (DisplayText != null ? DisplayText.GetHashCode() : 0);
			}
		}
	}
}