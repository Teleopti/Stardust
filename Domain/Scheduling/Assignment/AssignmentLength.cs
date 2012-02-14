using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    /// <summary>
    /// Restriction for length of assignment
    /// </summary>
    public class AssignmentLength : IRestriction<IPersonAssignment>
    {
        // Ola 2008-08-12 Changed from 36 to 24 after consulting Micke
        // Zoë 2009-11-12 Changed to 36 from 24 according to SBI 8690
        private const int _maxAssignmentLength = 36;

        #region IRestriction<PersonAssignment> Members

        /// <summary>
        /// Checks the entity.
        /// </summary>
        /// <param name="entityToCheck">The entity to check.</param>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-07
        /// </remarks>
        public void CheckEntity(IPersonAssignment entityToCheck)
        {
            double currentAssignmentLength = entityToCheck.Period.ElapsedTime().TotalHours;
            if (currentAssignmentLength > _maxAssignmentLength)
                throw new ValidationException(
                    string.Format(CultureInfo.CurrentCulture, UserTexts.Resources.BusinessRuleErrorAssignmentLength,
                                  _maxAssignmentLength, currentAssignmentLength));
        }

        #endregion
    }
}