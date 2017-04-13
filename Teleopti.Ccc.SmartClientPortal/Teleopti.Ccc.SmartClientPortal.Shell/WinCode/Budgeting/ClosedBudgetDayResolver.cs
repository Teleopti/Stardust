using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.WinCode.Budgeting.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Budgeting
{
    public class ClosedBudgetDayResolver
    {
        private IList<ISkillDay> _closedDays;
        private Dictionary<DateOnly, IBudgetGroupDayDetailModel> _selectedDsysdic;

        public ClosedBudgetDayResolver(IEnumerable<IBudgetGroupDayDetailModel> selectedBudgetDays, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysForSkills)
        {

            _closedDays = new List<ISkillDay>();

            //***
            //What this does is to find skilldays that are closed on all skills
            //This is tested and working... but if anyone has a faster or smarter way... please...
            //My vacation starts now, need to go...

            var days = from s in skillDaysForSkills.Values
                          from d in s
                          group d by d.CurrentDate into g
                          select new { Date = g.Key, SkillDays = g };
            
            
            foreach (var g in days.Where(g => g.SkillDays.All(d => !d.OpenForWork.IsOpen)))
            {
                _closedDays.Add(g.SkillDays.First());
            }
            //***

            _selectedDsysdic = selectedBudgetDays.ToDictionary(t => t.Date.Date);

        }

        public void InjectClosedDaysFromSkillDays()
        {
            foreach (var closedDay in _closedDays)
            {
                var key = closedDay.CurrentDate; //The timezone of the skill
                
                if (_selectedDsysdic.ContainsKey(key))
                {
                     _selectedDsysdic[key].IsClosed = true; 
                }
            }
        }
    }
}
