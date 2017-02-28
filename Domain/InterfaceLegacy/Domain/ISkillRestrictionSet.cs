namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// A set of restrictions for domain object Skill
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-07
    /// </remarks>
    public interface ISkillRestrictionSet : IRestrictionSet<ISkill>
    {
        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        void CheckEntity(object entityToCheck);

    }
}
