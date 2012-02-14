using System.Collections.Generic;
using System.Resources;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades
{


    public class ShiftTradeSkillSpecification : ShiftTradeSpecification, IShiftTradeSkillSpecification
    {
        public override string DenyReason
        {
            get
            {
              return  "ShiftTradeSkillDenyReason";
            }
        }
      
        public override bool IsSatisfiedBy(IList<IShiftTradeSwapDetail> obj)
        {
            foreach (var shiftTradeDetail in obj)
            {
                IWorkflowControlSet controlSetFrom = shiftTradeDetail.PersonFrom.WorkflowControlSet;
                IWorkflowControlSet controlSetTo = shiftTradeDetail.PersonTo.WorkflowControlSet;
                if (controlSetFrom == null || controlSetTo == null)
                    return false;
                IList<ISkill> mustMatchingSkills = getListOfSkills(controlSetFrom.MustMatchSkills,
                                                                   controlSetTo.MustMatchSkills);
                if (mustMatchingSkills.Count == 0)
                    return true;


                IPersonPeriod periodFrom = shiftTradeDetail.PersonFrom.Period(shiftTradeDetail.DateFrom);
                IPersonPeriod periodTo = shiftTradeDetail.PersonTo.Period(shiftTradeDetail.DateTo);
                if (periodFrom == null || periodTo == null)
                    return false;

                ICollection<ISkill> skills = new HashSet<ISkill>();
                foreach (var personSkill in periodFrom.PersonSkillCollection)
                {
                    if (mustMatchingSkills.Contains(personSkill.Skill))
                        skills.Add(personSkill.Skill);
                }

                foreach (var personSkill in periodTo.PersonSkillCollection)
                {
                    if (mustMatchingSkills.Contains(personSkill.Skill))
                    {
                        if (skills.Contains(personSkill.Skill))
                        {
                            skills.Remove(personSkill.Skill);
                        }
                        else
                        {
                            skills.Add(personSkill.Skill);
                        }
                    }
                }
                if (skills.Count > 0) return false; //These skills are the difference...
            }
            return true;
        }

        private static IList<ISkill> getListOfSkills(IEnumerable<ISkill> listOne, IEnumerable<ISkill> listTwo)
        {
            IList<ISkill> listOfSkills = new List<ISkill>();
            foreach (var skill in listOne)
            {
                if (!listOfSkills.Contains(skill))
                    listOfSkills.Add(skill);
            }
            foreach (var skill in listTwo)
            {
                if (!listOfSkills.Contains(skill))
                    listOfSkills.Add(skill);
            }
            return listOfSkills;
        }
    }
}
