using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "ShiftWork")]
    public class WorkShiftWorkTime : IWorkShiftWorkTime
	{
		private readonly IRuleSetProjectionService _ruleSetProjectionService;

		public WorkShiftWorkTime(IRuleSetProjectionService ruleSetProjectionService)
		{
			_ruleSetProjectionService = ruleSetProjectionService;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public IWorkTimeMinMax CalculateMinMax(IWorkShiftRuleSet workShiftRuleSet, IWorkTimeMinMaxRestriction restriction)
		{
			if (!restriction.MayMatchWithShifts())
				return null;

			var infoList = _ruleSetProjectionService.ProjectionCollection(workShiftRuleSet);

			IWorkTimeMinMax resultWorkTimeMinMax = null;
			var possibilities = new HashSet<IPossibleStartEndCategory>();
		    if (workShiftRuleSet != null)
		    {
		        var cat = workShiftRuleSet.TemplateGenerator.Category;
		        foreach (var visualLayerInfo in infoList)
		        {
			        if (!restriction.Match(visualLayerInfo)) continue;

			        var contractTime = visualLayerInfo.ContractTime;
			        var thisWorkTimeMinMax = new WorkTimeMinMax();
			        var period = visualLayerInfo.TimePeriod;
			        possibilities.Add(new PossibleStartEndCategory
				        {
					        StartTime = period.StartTime,
					        EndTime = period.EndTime,
					        ShiftCategory = cat
				        });
			        thisWorkTimeMinMax.StartTimeLimitation = new StartTimeLimitation(period.StartTime, period.StartTime);
			        thisWorkTimeMinMax.EndTimeLimitation = new EndTimeLimitation(period.EndTime, period.EndTime);
			        thisWorkTimeMinMax.WorkTimeLimitation = new WorkTimeLimitation(contractTime, contractTime);
					
			        if (resultWorkTimeMinMax == null)
				        resultWorkTimeMinMax = new WorkTimeMinMax();

			        resultWorkTimeMinMax = resultWorkTimeMinMax.Combine(thisWorkTimeMinMax);
		        }
		    }
		    if (resultWorkTimeMinMax != null)
				resultWorkTimeMinMax.PossibleStartEndCategories = possibilities.ToList();
			return resultWorkTimeMinMax;
		}
	}
}