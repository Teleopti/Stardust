using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
    public class NewSkillStaffPeriodValues : INewSkillStaffPeriodValues
    {
        private readonly IEnumerable<ISkillStaffPeriod> _updatedSkillStaffPeriods;
        private Delegate _runWhenBatchCompleted;

        public NewSkillStaffPeriodValues(IEnumerable<ISkillStaffPeriod> updatedSkillStaffPeriods)
        {
            _updatedSkillStaffPeriods = updatedSkillStaffPeriods;
        }

        public void RunWhenBatchCompleted(Action<object> calculateChildSkillDay)
        {
            _runWhenBatchCompleted = calculateChildSkillDay;
        }

		public void BatchCompleted()
		{
			_runWhenBatchCompleted?.DynamicInvoke(this);
			_runWhenBatchCompleted = null;
		}

		public void SetValues(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            skillStaffPeriods.ForEach(s =>
                                          {
                                              s.Reset();
                                              s.IsAvailable = false;
                                          });
			var skillStaffPeriodForStartDateTime = skillStaffPeriods.ToDictionary(s => s.Period.StartDateTime);
            foreach (ISkillStaffPeriod skillStaffPeriod in _updatedSkillStaffPeriods)
            {
                if (!skillStaffPeriodForStartDateTime.TryGetValue(skillStaffPeriod.Period.StartDateTime, out var currentPeriod)) continue;

                currentPeriod.Payload.TaskData = skillStaffPeriod.Payload.TaskData;
                currentPeriod.Payload.ServiceAgreementData = skillStaffPeriod.Payload.ServiceAgreementData;
                currentPeriod.Payload.SkillPersonData = skillStaffPeriod.Payload.SkillPersonData;
                currentPeriod.Payload.Shrinkage = skillStaffPeriod.Payload.Shrinkage;
                currentPeriod.Payload.Efficiency = skillStaffPeriod.Payload.Efficiency;
                currentPeriod.Payload.ManualAgents = skillStaffPeriod.Payload.ManualAgents;
                currentPeriod.IsAvailable = true;
            }
        }
    }
}