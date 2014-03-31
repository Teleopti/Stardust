using System.Drawing;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class ActivityCreator
    {
        public ActivityCreator()
        {
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="color">The color.</param>
        /// <param name="inReadyTime">if set to <c>true</c> [in ready time].</param>
        /// <param name="inContractTime">if set to <c>true</c> [in contract time].</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: peterwe
        /// Created date: 2008-11-11
        /// </remarks>
        public IActivity Create(string name, Description description, Color color, bool inReadyTime, bool inContractTime)
        {
            IActivity activity = new Activity(name)
                                     {
                                         Description = description,
                                         DisplayColor = color,
                                         InReadyTime = inReadyTime,
                                         InContractTime = inContractTime,
                                     };
            return activity;
        }
    }
}
