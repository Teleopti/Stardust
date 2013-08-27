using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftPerAgentGrid
	{
		
	}

	public class ShiftPerAgentGridPresenter
	{
		private IShiftPerAgentGrid _view;

		public ShiftPerAgentGridPresenter(IShiftPerAgentGrid view)
		{
			_view = view;
		}

		public int ShiftCategoryCount(IPerson person, IShiftCategory shiftCategory, IList<ShiftCategoryPerAgent> shiftCategoryPerAgentList)
		{
			return (from shiftCategoryPerAgent in shiftCategoryPerAgentList 
					where shiftCategoryPerAgent.Person.Equals(person) 
					where shiftCategoryPerAgent.ShiftCategory.Equals(shiftCategory) 
					select shiftCategoryPerAgent.Count).FirstOrDefault();
		}
	}
}
