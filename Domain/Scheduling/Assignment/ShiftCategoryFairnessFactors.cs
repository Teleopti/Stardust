using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class ShiftCategoryFairnessFactors : IShiftCategoryFairnessFactors
    {
        private readonly IDictionary<IShiftCategory, double> _dictionary;
    	private readonly double _fairnessPointsPerShift;

    	public ShiftCategoryFairnessFactors(IDictionary<IShiftCategory, double> dictionary, double fairnessPointsPerShift)
        {
        	_dictionary = dictionary;
        	_fairnessPointsPerShift = fairnessPointsPerShift;
        }

    	public double FairnessPointsPerShift
    	{
			get { return _fairnessPointsPerShift; }
    	}


    	public double FairnessFactor(IShiftCategory shiftCategory)
        {
        	double factor;
			if (!_dictionary.TryGetValue(shiftCategory, out factor))
				return 0;

			return factor;
        }
    }
}