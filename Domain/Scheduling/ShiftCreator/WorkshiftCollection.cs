using System;
using System.Collections.ObjectModel;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class WorkShiftCollection : Collection<IWorkShift>
	{
		public WorkShiftCollection(IWorkShiftAddCallback callback)
		{
			Callback = callback;
		}

		protected override void InsertItem(int index, IWorkShift item)
		{
			Callback?.BeforeAdd(item);
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			Callback?.BeforeRemove();
			base.RemoveItem(index);
		}
		
		public IWorkShiftAddCallback Callback { get; }
	}
}