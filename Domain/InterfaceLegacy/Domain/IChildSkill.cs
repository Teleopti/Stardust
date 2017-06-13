namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Represents a child skill.
    /// </summary>
    public interface IChildSkill: ISkill
    {
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