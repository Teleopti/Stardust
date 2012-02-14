using System.Collections.Generic;
using System.Globalization;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IGapsInAssignment : IRestriction<IPersonAssignment>{}

    public class GapsInAssignment : IGapsInAssignment
    {
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
            IProjectionService projectionService = entityToCheck.ProjectionService();
            IList<IVisualLayer> layerColl = new List<IVisualLayer>(projectionService.CreateProjection());
            var gap = new DateTimePeriod();

            bool failed = false;
            if (layerColl.Count > 1)
            {
                for (int index = 1; index < layerColl.Count; index++)
                {
                    if (layerColl[index - 1].Period.EndDateTime < layerColl[index].Period.StartDateTime)
                    {
                        failed = true;
                        gap = new DateTimePeriod(layerColl[index - 1].Period.EndDateTime, layerColl[index].Period.StartDateTime);
                    }
                } 
            }
            
            if (failed)
            {
                throw new ValidationException(
                    string.Format(CultureInfo.CurrentCulture, entityToCheck.Person.Name + ". " + UserTexts.Resources.GapExists,
                                  gap.ToLocalString()));
            }
        }

        #endregion
    }
}