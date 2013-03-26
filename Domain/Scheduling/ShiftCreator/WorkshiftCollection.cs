using Teleopti.Interfaces.Domain;
using System.Collections.ObjectModel;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
	public class WorkShiftCollection : Collection<IWorkShift>
	{
		
		public WorkShiftCollection(IWorkShiftAddCallback  callback)
		{
			Callback = callback;
		}

		protected override void InsertItem(int index, IWorkShift item)
		{
			if(Callback != null)
				Callback.BeforeAdd(item);
			base.InsertItem(index, item);
		}

		protected override void RemoveItem(int index)
		{
			if (Callback != null)
				Callback.BeforeRemove();
			base.RemoveItem(index);
			
		}

		

		public IWorkShiftAddCallback Callback { get; private set; }
	}

	
}