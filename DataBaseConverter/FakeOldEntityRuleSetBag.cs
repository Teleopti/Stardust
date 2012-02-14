using System.Collections.ObjectModel;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter
{
    /// <summary>
    /// Simulate oldEntity when converting data to WorkRuleSet from old shiftclasses
    /// </summary>
    public class FakeOldEntityRuleSetBag
    {
        private global::Domain.Unit _unit;
        private global::Domain.EmploymentType _employmentType;
        private string _description;
        private Collection<IWorkShiftRuleSet> _workShiftRuleSets;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="employmentType"></param>
        public FakeOldEntityRuleSetBag(global::Domain.Unit unit, global::Domain.EmploymentType employmentType)
        {
            _unit = unit;
            _employmentType = employmentType;
            _description = unit.Name + " " + employmentType.Description;
            _workShiftRuleSets = new Collection<IWorkShiftRuleSet>();
        }

        /// <summary>
        /// Unit
        /// </summary>
        public global::Domain.Unit Unit
        {
            get { return _unit; }
        }

        /// <summary>
        /// EmploymentType
        /// </summary>
        public global::Domain.EmploymentType EmploymentType
        {
            get { return _employmentType; }
        }

        /// <summary>
        /// Description
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        /// <summary>
        /// Add workshift rule set
        /// </summary>
        /// <param name="ruleSet"></param>
        public void AddWorkShiftRuleSet(IWorkShiftRuleSet ruleSet)
        {
            _workShiftRuleSets.Add(ruleSet);
        }

        /// <summary>
        /// return list with rulesets
        /// </summary>
        public Collection<IWorkShiftRuleSet> WorkShiftRuleSets
        {
            get { return _workShiftRuleSets; }
        }
    }
}
