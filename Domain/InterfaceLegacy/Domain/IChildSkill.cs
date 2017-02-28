namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Represents a child skill.
    /// </summary>
    public interface IChildSkill: ISkill
    {
        /// <summary>
        /// Sets the parent skill.
        /// </summary>
        /// <param name="parentSkill">The parent skill.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        void SetParentSkill(IMultisiteSkill parentSkill);

        /// <summary>
        /// Gets the parent skill.
        /// </summary>
        /// <value>The parent skill.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-04-21
        /// </remarks>
        IMultisiteSkill ParentSkill { get; }
    }
}