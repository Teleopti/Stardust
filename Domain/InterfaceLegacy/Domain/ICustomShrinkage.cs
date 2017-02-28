using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// A custom shrinkage item
    ///</summary>
    public interface ICustomShrinkage : IAggregateEntity
    {
        ///<summary>
        /// Gets or sets the name of this custom shrinkage item
        ///</summary>
        string ShrinkageName { get; set; }
        ///<summary>
        /// Shrinkage included in request allowance
        ///</summary>
        bool IncludedInAllowance { get; set; }
        /// <summary>
        /// Index of this custom shrinkage
        /// </summary>
        int OrderIndex { get; set; }
        /// <summary>
        /// Absences included in this custom shrinkage
        /// </summary>
        IEnumerable<IAbsence> BudgetAbsenceCollection { get; }
        /// <summary>
        /// Add an absence to this custom shrinkage
        /// </summary>
        /// <param name="absence">Absence</param>
        void AddAbsence(IAbsence absence);
        /// <summary>
        /// Remvoe all absences from this custom shrinkage
        /// </summary>
        void RemoveAllAbsences();
    }
}