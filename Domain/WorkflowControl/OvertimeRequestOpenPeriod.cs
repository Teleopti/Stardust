using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public abstract class OvertimeRequestOpenPeriod : AggregateEntity, IOvertimeRequestOpenPeriod
	{
		private int _orderIndex;
		private OvertimeRequestAutoGrantType _autoGrantType;
		public abstract DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly);

		public virtual OvertimeRequestAutoGrantType AutoGrantType
		{
			get => _autoGrantType;
			set => _autoGrantType = value;
		}
	

		public virtual int OrderIndex
		{
			get
			{
				var owner = Parent as IWorkflowControlSet;
				_orderIndex = owner?.OvertimeRequestOpenPeriods.IndexOf(this) ?? -1;
				return _orderIndex;
			}
			set => _orderIndex = value;
		}

		public virtual string DenyReason { get; set; }

		public virtual object Clone()
		{
			return NoneEntityClone();
		}

		public virtual IOvertimeRequestOpenPeriod NoneEntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			clone.SetId(null);
			return clone;
		}

		public virtual IOvertimeRequestOpenPeriod EntityClone()
		{
			var clone = (OvertimeRequestOpenPeriod)MemberwiseClone();
			return clone;
		}
	}

	public enum OvertimeRequestAutoGrantType
	{
		No,
		Yes,
		Deny
	}

	public class OvertimeRequestOpenRollingPeriod : OvertimeRequestOpenPeriod
	{
		private MinMax<int> _betweenDays;
		public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
		{
			return new DateOnlyPeriod(viewpointDateOnly.AddDays(_betweenDays.Minimum), viewpointDateOnly.AddDays(_betweenDays.Maximum));
		}

		public virtual MinMax<int> BetweenDays
		{
			get => _betweenDays;
			set => _betweenDays = value;
		}
	}

	public class OvertimeRequestOpenDatePeriod : OvertimeRequestOpenPeriod
	{
		private DateOnlyPeriod _period;
		public override DateOnlyPeriod GetPeriod(DateOnly viewpointDateOnly)
		{
			return _period;
		}

		public virtual DateOnlyPeriod Period
		{
			get => _period;
			set => _period = value;
		}
	}
}
