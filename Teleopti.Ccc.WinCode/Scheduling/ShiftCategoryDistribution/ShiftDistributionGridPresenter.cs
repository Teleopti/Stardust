using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ShiftCategoryDistribution
{
	public interface IShiftDistributionGrid
	{
		
	}

	public class ShiftDistributionGridPresenter
	{
		private IShiftDistributionGrid _view;
	    private readonly IList<ShiftDistribution> _shiftDistributionList;

	    public ShiftDistributionGridPresenter(IShiftDistributionGrid view, IList<ShiftDistribution> shiftDistributionList)
		{
		    _view = view;
		    _shiftDistributionList = shiftDistributionList;
		}

	    public int? ShiftCategoryCount(DateOnly date, IShiftCategory shiftCategory)
	    {
	        foreach (var shiftDistribution in _shiftDistributionList)
	        {
	            if (shiftDistribution.DateOnly.Equals(date))
	            {
	                if (shiftDistribution.ShiftCategory.Equals(shiftCategory))
	                {
	                    return  shiftDistribution.Count;
	                }
	            }
	        }
	        return null;
	    }
	}
}
