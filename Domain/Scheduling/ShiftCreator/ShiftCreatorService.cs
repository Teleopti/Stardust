﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.ShiftCreator
{
    public class ShiftCreatorService : IShiftCreatorService
    {
        private readonly ICreateWorkShiftsFromTemplate _shiftGenerator;

        public ShiftCreatorService(ICreateWorkShiftsFromTemplate shiftGenerator)
        {
            _shiftGenerator = shiftGenerator;
        }

        public ICreateWorkShiftsFromTemplate WorkShiftGenerator
        {
            get { return _shiftGenerator; }
        }

		public IList<IWorkShift> Generate(IWorkShiftRuleSet ruleSet, IWorkShiftAddCallback callback)
	    {
			var retColl = new List<IWorkShift>();
			using (PerformanceOutput.ForOperation("Generating workshifts for " + ruleSet.Description.Name))
			{
				var templates = ruleSet.TemplateGenerator.Generate();
				
				foreach (var template in templates)
				{
					if (callback != null && callback.IsCanceled)
						break;
					retColl.AddRange(_shiftGenerator.Generate(template, ruleSet.ExtenderCollection,
															  ruleSet.LimiterCollection, callback));
				}
			}
			return retColl;
	    }
    }
}