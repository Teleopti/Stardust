using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Sets the ScheduleTag on a ScheduleDay according go rules
    /// </summary>
    public interface IScheduleTagSetter
    {
        /// <summary>
        /// Sets the tag.
        /// </summary>
        /// <param name="modifier">The modifier.</param>
        /// <param name="scheduleParts">The schedule parts.</param>
        void SetTagOnScheduleDays(ScheduleModifier modifier, IEnumerable<IScheduleDay> scheduleParts);

        /// <summary>
        /// Changes the tag to set.
        /// </summary>
        /// <param name="tag">The tag.</param>
        void ChangeTagToSet(IScheduleTag tag);
    }

}