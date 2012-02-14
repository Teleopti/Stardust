using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DatabaseConverter.EntityMapper
{
    /// <summary>
    /// Mapper for RuleSetBag
    /// </summary>
    public class RuleSetBagMapper : Mapper<IRuleSetBag, FakeOldEntityRuleSetBag>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="mappedObjectPair"></param>
        /// <param name="timeZone"></param>
        public RuleSetBagMapper(MappedObjectPair mappedObjectPair, ICccTimeZoneInfo timeZone) : base(mappedObjectPair, timeZone)
        {
            
        }

        /// <summary>
        /// Map
        /// </summary>
        /// <param name="oldEntity"></param>
        /// <returns></returns>
        public override IRuleSetBag Map(FakeOldEntityRuleSetBag oldEntity)
        {
            RuleSetBag ruleSetBag = new RuleSetBag();
            Description description = new Description(oldEntity.Description); 
            ruleSetBag.Description = description;
            
            if (oldEntity.Unit.Deleted)
                return null;

            foreach (WorkShiftRuleSet ruleSet in oldEntity.WorkShiftRuleSets)
            {
                if(ruleSet != null)
                {
                    ruleSetBag.AddRuleSet(ruleSet);
                }
                    
            }
            if (ruleSetBag.RuleSetCollection.Count == 0)
                return null;

            MappedObjectPair.RuleSetBag.Add(new UnitEmploymentType(oldEntity.Unit, oldEntity.EmploymentType), ruleSetBag);
            return ruleSetBag;
        }
    }
}
