using System.Collections.ObjectModel;

namespace Teleopti.Interfaces.Domain
{
    /// <summary>
    /// Class Grouping absence
    /// </summary>
    public interface IGroupingAbsence : IAggregateRoot
    {
        /// <summary>
        /// Set/Get for Description
        /// </summary>     
        Description Description { get; set; }

        /// <summary>
        /// Name Property
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// ShortName Property
        /// </summary>
        string ShortName { get; set; }

        /// <summary>
        /// Gets the absence types.
        /// Read only wrapper around the actual absence list.
        /// </summary>
        /// <value>The absence types list.</value>
        ReadOnlyCollection<IAbsence> AbsenceCollection { get; }

        /// <summary>
        /// Adds an Absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        void AddAbsence(IAbsence absence);

        /// <summary>
        /// Remove an Absence.
        /// </summary>
        /// <param name="absence">The absence.</param>
        void RemoveAbsence(IAbsence absence);
    }
}
