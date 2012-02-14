using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MainShift : Shift, IMainShift
	{
		private IShiftCategory _shiftCategory;

		public MainShift(IShiftCategory category)
		{
			InParameter.NotNull("category", category);
			_shiftCategory = category;
		}

		protected MainShift()
		{
		}

		public virtual IShiftCategory ShiftCategory
		{
			get
			{
				return _shiftCategory;
			}
			set
			{
				InParameter.NotNull("value", value);
				_shiftCategory = value;
			}
		}

		public override void OnAdd(ILayer<IActivity> layer)
		{
			if (!(layer is MainShiftActivityLayer))
				throw new ArgumentException("Only MainShiftActivityLayers can be added to a MainShift");
		}

		public override bool Equals(IEntity other)
		{
			//to prevent equal with PersonAssignment (has same Id)
			if (!(other is IMainShift))
				return false;
			return base.Equals(other);
		}
	}
}