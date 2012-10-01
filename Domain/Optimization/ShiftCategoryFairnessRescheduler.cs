using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessRescheduler
	{
		bool Execute(IList<IPerson> persons);
	}
	public class ShiftCategoryFairnessRescheduler : IShiftCategoryFairnessRescheduler
	{
		public bool Execute(IList<IPerson> persons)
		{
			throw new System.NotImplementedException();
		}
	}
}