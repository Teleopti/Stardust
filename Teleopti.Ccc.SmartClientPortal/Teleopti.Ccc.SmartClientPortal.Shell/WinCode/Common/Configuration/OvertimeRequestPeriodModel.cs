using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common.Configuration
{
	public class OvertimeRequestPeriodModel
	{
		public WorkflowControlSetModel Owner { get; set; }
		private IOvertimeRequestOpenPeriod _overtimeRequestOpenPeriod;
		private OvertimeRequestPeriodTypeModel _periodType;

		public OvertimeRequestPeriodModel(IOvertimeRequestOpenPeriod absenceRequestOpenPeriod, WorkflowControlSetModel owner)
		{
			Owner = owner;
			SetDomainEntity(absenceRequestOpenPeriod);
		}

		public IOvertimeRequestOpenPeriod DomainEntity => _overtimeRequestOpenPeriod;

		public OvertimeRequestPeriodTypeModel PeriodType => _periodType;

		public OvertimeRequestAutoGrantType OvertimeRequestAutoGrantType
		{
			get => _overtimeRequestOpenPeriod.AutoGrantType;
			set
			{
				_overtimeRequestOpenPeriod.AutoGrantType = value;
				Owner.IsDirty = true;
			}
		}

		public int? RollingStart
		{
			get
			{
				var period = _overtimeRequestOpenPeriod as OvertimeRequestOpenRollingPeriod;
				if (period == null) return null;
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
				var period = _overtimeRequestOpenPeriod as OvertimeRequestOpenRollingPeriod;
				if (period == null) return null;
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
				var period = _overtimeRequestOpenPeriod as OvertimeRequestOpenDatePeriod;
				if (period == null) return null;
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
				var period = _overtimeRequestOpenPeriod as OvertimeRequestOpenDatePeriod;
				if (period == null) return null;
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

		public void SetDomainEntity(IOvertimeRequestOpenPeriod absenceRequestOpenPeriod)
		{
			_overtimeRequestOpenPeriod = absenceRequestOpenPeriod;
			_periodType = new OvertimeRequestPeriodTypeModel(_overtimeRequestOpenPeriod, string.Empty);
			foreach (var periodType in WorkflowControlSetModel.DefaultOvertimeRequestPeriodAdapters)
			{
				if (_periodType.Equals(periodType))
					_periodType.DisplayText = periodType.DisplayText;
			}
		}
	}
}