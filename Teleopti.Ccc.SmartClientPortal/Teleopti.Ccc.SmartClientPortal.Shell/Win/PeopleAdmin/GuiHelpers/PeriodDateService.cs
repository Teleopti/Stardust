using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.PeopleAdmin.GuiHelpers
{
    /// <summary>
    /// This is used for validating period dates in period grids.
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2/9/2009
    /// </remarks>
    public static class PeriodDateService
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public static DateOnly GetValidPeriodDate(ICollection<DateOnly> dateDictionary, DateOnly subjectDate)
        {

            while (true)
            {
                if (!dateDictionary.Contains(subjectDate))
                {
                    return subjectDate;
                }

                subjectDate = subjectDate.AddDays(1);

            }
        }
    }
}
