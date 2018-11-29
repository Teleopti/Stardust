using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings;
using Teleopti.Ccc.UserTexts;

using HandleOptionViewDictionary = System.Collections.Generic.Dictionary<Teleopti.Ccc.Domain.WorkflowControl.OvertimeValidationHandleType, Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Settings.OvertimeRequestValidationHandleOptionView>;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class OvertimeRequestPeriodModel
	{
		public WorkflowControlSetModel Owner { get; set; }
		private IOvertimeRequestOpenPeriod _overtimeRequestOpenPeriod;
		private OvertimeRequestPeriodTypeModel _periodType;
		private OvertimeRequestValidationHandleOptionView _workRuleValidationHandleType;
		private string _skillTypes;

		internal static readonly HandleOptionViewDictionary
			OvertimeRequestWorkRuleValidationHandleOptionViews
				= new HandleOptionViewDictionary
				{
					{
						OvertimeValidationHandleType.Pending,
						new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Pending,
							Resources.SendToAdministrator)
					},
					{
						OvertimeValidationHandleType.Deny,
						new OvertimeRequestValidationHandleOptionView(OvertimeValidationHandleType.Deny, Resources.Deny)
					}
				};

		public OvertimeRequestPeriodModel(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod, WorkflowControlSetModel owner)
		{
			Owner = owner;
			SetDomainEntity(overtimeRequestOpenPeriod);
		}

		public IOvertimeRequestOpenPeriod DomainEntity => _overtimeRequestOpenPeriod;

		public OvertimeRequestPeriodTypeModel PeriodType => _periodType;

		public string SkillTypes
		{
			get => _skillTypes;
			set
			{
				if (_skillTypes.Equals(value))
				{
					return;
				}
				_skillTypes = value;
				updatePeriodSkillTypes();
			}
		}

		public OvertimeRequestAutoGrantType AutoGrantType
		{
			get => _overtimeRequestOpenPeriod.AutoGrantType;
			set
			{
				_overtimeRequestOpenPeriod.AutoGrantType = value;
				Owner.IsDirty = true;
			}
		}

		public OvertimeRequestValidationHandleOptionView WorkRuleValidationHandleType
		{
			get => _workRuleValidationHandleType;
			set
			{
				_workRuleValidationHandleType = value;
				_overtimeRequestOpenPeriod.WorkRuleValidationHandleType = value?.WorkRuleValidationHandleType;
				Owner.IsDirty = true;
			}
		}

		public bool EnableWorkRuleValidation
		{
			get => _overtimeRequestOpenPeriod.EnableWorkRuleValidation;
			set
			{
				_overtimeRequestOpenPeriod.EnableWorkRuleValidation = value;
				Owner.IsDirty = true;
			}
		}

		public int? RollingStart
		{
			get
			{
				if (!(_overtimeRequestOpenPeriod is OvertimeRequestOpenRollingPeriod period)) return null;
				return period.BetweenDays.Minimum;
			}
			set
			{
				if (value.HasValue)
				{
					var period = (OvertimeRequestOpenRollingPeriod)_overtimeRequestOpenPeriod;
					var currentEndDay = period.BetweenDays.Maximum;
					if (currentEndDay < value)
					{
						currentEndDay = value.Value;
					}
					period.BetweenDays = new MinMax<int>(value.Value, currentEndDay);
					Owner.IsDirty = true;
				}
			}
		}

		public int? RollingEnd
		{
			get
			{
				if (!(_overtimeRequestOpenPeriod is OvertimeRequestOpenRollingPeriod period)) return null;
				return period.BetweenDays.Maximum;
			}
			set
			{
				if (value.HasValue)
				{
					var period = (OvertimeRequestOpenRollingPeriod)_overtimeRequestOpenPeriod;
					var currentStartDay = period.BetweenDays.Minimum;
					if (currentStartDay > value)
					{
						currentStartDay = value.Value;
					}
					period.BetweenDays = new MinMax<int>(currentStartDay, value.Value);
					Owner.IsDirty = true;
				}
			}
		}

		public DateOnly? PeriodStartDate
		{
			get
			{
				if (!(_overtimeRequestOpenPeriod is OvertimeRequestOpenDatePeriod period)) return null;
				return period.Period.StartDate;
			}
			set
			{
				if (value.HasValue)
				{
					var period = (OvertimeRequestOpenDatePeriod)_overtimeRequestOpenPeriod;
					var currentEndDate = period.Period.EndDate;
					if (currentEndDate < value.Value)
					{
						currentEndDate = value.Value;
					}
					period.Period = new DateOnlyPeriod(value.Value, currentEndDate);
					Owner.IsDirty = true;
				}
			}
		}

		public DateOnly? PeriodEndDate
		{
			get
			{
				if (!(_overtimeRequestOpenPeriod is OvertimeRequestOpenDatePeriod period)) return null;
				return period.Period.EndDate;
			}
			set
			{
				if (value.HasValue)
				{
					var period = (OvertimeRequestOpenDatePeriod)_overtimeRequestOpenPeriod;
					var currentStartDate = period.Period.StartDate;
					if (currentStartDate > value.Value)
					{
						currentStartDate = value.Value;
					}
					period.Period = new DateOnlyPeriod(currentStartDate, value.Value);
					Owner.IsDirty = true;
				}
			}
		}

		public void SetDomainEntity(IOvertimeRequestOpenPeriod overtimeRequestOpenPeriod)
		{
			_overtimeRequestOpenPeriod = overtimeRequestOpenPeriod;
			_periodType = new OvertimeRequestPeriodTypeModel(_overtimeRequestOpenPeriod, string.Empty);

			_skillTypes = overtimeRequestOpenPeriod.PeriodSkillTypes != null && overtimeRequestOpenPeriod.PeriodSkillTypes.Any()
				? string.Join(",", overtimeRequestOpenPeriod.PeriodSkillTypes.Select(s => Resources.ResourceManager.GetString(s.SkillType.Description.Name)).Distinct())
				: Resources.SkillTypeInboundTelephony;
			
			foreach (var periodType in WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters)
			{
				if (_periodType.Equals(periodType))
					_periodType.DisplayText = periodType.DisplayText;
			}

			if (overtimeRequestOpenPeriod.WorkRuleValidationHandleType.HasValue)
			{
				_workRuleValidationHandleType =
					OvertimeRequestWorkRuleValidationHandleOptionViews[overtimeRequestOpenPeriod.WorkRuleValidationHandleType.Value];
			}
			else
			{
				_workRuleValidationHandleType = null;
			}
		}

		private void updatePeriodSkillTypes()
		{
			_overtimeRequestOpenPeriod.ClearPeriodSkillType();

			var supportedSkillTypes = WorkflowControlSetModel.GetSupportedSkillTypes();
			var originSkillTypes = _skillTypes.Split(',').Distinct()
				.Select(skillType =>
					supportedSkillTypes.FirstOrDefault(s =>
						Resources.ResourceManager.GetString(s.Description.Name).Equals(skillType)))
				.Where(s => s != null)
				.Select(s => new OvertimeRequestOpenPeriodSkillType(s));
			originSkillTypes.ForEach(_overtimeRequestOpenPeriod.AddPeriodSkillType);

			Owner.IsDirty = true;
		}
	}
}