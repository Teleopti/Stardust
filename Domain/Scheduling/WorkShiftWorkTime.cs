using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
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
			var callback = new WorkShiftAddStopperCallback();
			callback.StartNewRuleSet(workShiftRuleSet);
			var infoList = _ruleSetProjectionService.ProjectionCollection(workShiftRuleSet, callback);

			IWorkTimeMinMax resultWorkTimeMinMax = null;
		    if (workShiftRuleSet != null)
		    {
		        foreach (var visualLayerInfo in infoList)
		        {
			        if (!restriction.Match(visualLayerInfo)) continue;

			        var contractTime = visualLayerInfo.ContractTime;
			        var thisWorkTimeMinMax = new WorkTimeMinMax();
			        var period = visualLayerInfo.TimePeriod;
			        thisWorkTimeMinMax.StartTimeLimitation = new StartTimeLimitation(period.StartTime, period.StartTime);
			        thisWorkTimeMinMax.EndTimeLimitation = new EndTimeLimitation(period.EndTime, period.EndTime);
			        thisWorkTimeMinMax.WorkTimeLimitation = new WorkTimeLimitation(contractTime, contractTime);
					
			        if (resultWorkTimeMinMax == null)
				        resultWorkTimeMinMax = new WorkTimeMinMax();

			        resultWorkTimeMinMax = resultWorkTimeMinMax.Combine(thisWorkTimeMinMax);
		        }
		    }

			return resultWorkTimeMinMax;
		}
	}
}