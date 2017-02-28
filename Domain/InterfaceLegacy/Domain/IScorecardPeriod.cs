namespace Teleopti.Interfaces.Domain
{
    ///<summary>
    /// Class holding what Period the scorecard should show
    ///</summary>
    public interface IScorecardPeriod
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-18    
        /// </remarks>
        string Name { get; }

        /// <summary>
        /// Gets the id.
        /// </summary>
        /// <value>The id.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-18    
        /// </remarks>
        int Id { get; }
    }
}