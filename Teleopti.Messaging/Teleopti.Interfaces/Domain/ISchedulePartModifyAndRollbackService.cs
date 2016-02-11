using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Used for modifying and rolling back schedule changes.
    ///</summary>
    public interface ISchedulePartModifyAndRollbackService
    {
	    void Modify(IScheduleDay schedulePart);
	    void Modify(IScheduleDay schedulePart, INewBusinessRuleCollection newBusinessRuleCollection);

	    void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter);

	    void ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection);

	    void Rollback();
	    IEnumerable<IScheduleDay> ModificationCollection { get; }
	    void ClearModificationCollection();
	    IEnumerable<IBusinessRuleResponse> ModifyParts(IEnumerable<IScheduleDay> scheduleParts);
    }
}
