using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    ///<summary>
    /// Used for modifying and rolling back schedule changes.
    ///</summary>
    public interface ISchedulePartModifyAndRollbackService
    {
	    void Modify(IScheduleDay schedulePart);
	    void Modify(IScheduleDay schedulePart, INewBusinessRuleCollection newBusinessRuleCollection);

	    void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter);

	    bool ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection);
	    bool ModifyStrictlyRollbackWithoutValidation(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection);

	    void Rollback();
	    IEnumerable<IScheduleDay> ModificationCollection { get; }
	    void ClearModificationCollection();
	    IEnumerable<IBusinessRuleResponse> ModifyParts(IEnumerable<IScheduleDay> scheduleParts);
	    void RollbackMinimumChecks();
    }
}
