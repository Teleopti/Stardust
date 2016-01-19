using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

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
            if (_runWhenBatchCompleted!=null)
            {
                _runWhenBatchCompleted.DynamicInvoke(this);
                _runWhenBatchCompleted = null;
            }
        }

        public void SetValues(IEnumerable<ISkillStaffPeriod> skillStaffPeriods)
        {
            skillStaffPeriods.ForEach(s =>
                                          {
                                              s.Reset();
                                              s.IsAvailable = false;
                                          });
            foreach (ISkillStaffPeriod skillStaffPeriod in _updatedSkillStaffPeriods)
            {
                DateTime startDateTime = skillStaffPeriod.Period.StartDateTime;
                var currentPeriod =
                    skillStaffPeriods.FirstOrDefault(
                        s => s.Period.StartDateTime == startDateTime);
                if (currentPeriod == null) continue;

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