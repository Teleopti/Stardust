using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IShiftCategoryFairnessReScheduler
	{
		bool Execute(IList<IPerson> persons);
	}
	public class ShiftCategoryFairnessReScheduler : IShiftCategoryFairnessReScheduler
	{
		public bool Execute(IList<IPerson> persons)
		{
			throw new System.NotImplementedException();
		}
	}
}