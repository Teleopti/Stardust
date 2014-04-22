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

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
	    IEnumerable<IBusinessRuleResponse> Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter);

	    IEnumerable<IBusinessRuleResponse> Modify(IEnumerable<IScheduleDay> schedulePartList, IScheduleTagSetter scheduleTagSetter);

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
	    void Modify(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection);

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
	    void ModifyStrictly(IScheduleDay schedulePart, IScheduleTagSetter scheduleTagSetter, INewBusinessRuleCollection newBusinessRuleCollection);

	    void Rollback();
	    void RollbackLast();
	    int StackLength { get; }
	    IEnumerable<IScheduleDay> ModificationCollection { get; }
	    void ClearModificationCollection();

	    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
	    IEnumerable<IBusinessRuleResponse> ModifyParts(IEnumerable<IScheduleDay> scheduleParts);

	    IEnumerable<IBusinessRuleResponse> Modify(IEnumerable<IScheduleDay> schedulePartList, IScheduleTagSetter scheduleTagSetter);
    }
}
