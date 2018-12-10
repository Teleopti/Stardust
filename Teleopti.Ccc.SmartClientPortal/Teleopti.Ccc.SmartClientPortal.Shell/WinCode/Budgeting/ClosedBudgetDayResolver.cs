using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting.Models;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Budgeting
{
    public class ClosedBudgetDayResolver
    {
        private List<DateOnly> _closedDays;
        private Dictionary<DateOnly, IBudgetGroupDayDetailModel> _selectedDsysdic;

        public ClosedBudgetDayResolver(IEnumerable<IBudgetGroupDayDetailModel> selectedBudgetDays, IDictionary<ISkill, IEnumerable<ISkillDay>> skillDaysForSkills)
        {
            var days = from s in skillDaysForSkills.Values
                          from d in s
                          group d by d.CurrentDate into g
                          select new { Date = g.Key, Closed = g.All(d => !d.OpenForWork.IsOpen) };

	        _closedDays = days.Where(d => d.Closed).Select(d => d.Date).ToList();
            
            _selectedDsysdic = selectedBudgetDays.ToDictionary(t => t.Date.Date);

        }

        public void InjectClosedDaysFromSkillDays()
        {
            foreach (var closedDay in _closedDays)
            {
                if (_selectedDsysdic.ContainsKey(closedDay))
                {
                     _selectedDsysdic[closedDay].IsClosed = true; 
                }
            }
        }
    }
}
