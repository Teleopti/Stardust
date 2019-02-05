using System.Drawing;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Holds values for one Kpi and One Team
    /// </summary>
    public interface IKpiTarget : IAggregateRoot, IChangeInfo, IFilterOnBusinessUnit
    {
        /// <summary>
        /// Gets the team description.
        /// For readonly use in databindings.
        /// </summary>
        /// <value>The team description.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-10    
        /// </remarks>
        string TeamDescription { get; }

        /// <summary>
        /// Gets or sets the color of the between.
        /// </summary>
        /// <value>The color of the between.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-09    
        /// </remarks>
        Color BetweenColor { get; set; }

        /// <summary>
        /// Gets or sets the KeyPerformanceIndicator.
        /// </summary>
        /// <value>The KeyPerformanceIndicator.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        IKeyPerformanceIndicator KeyPerformanceIndicator { get; set; }

        /// <summary>
        /// Gets or sets the team.
        /// </summary>
        /// <value>The team.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        ITeam Team { get; set; }

        /// <summary>
        /// Gets or sets the target value.
        /// </summary>
        /// <value>The target value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        double TargetValue { get; set; }

        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        /// <value>The min value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        double MinValue { get; set; }

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        /// <value>The max value.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        double MaxValue { get; set; }

        /// <summary>
        /// Gets or sets the color of the min.
        /// </summary>
        /// <value>The color of the min.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        Color LowerThanMinColor { get; set; }

        /// <summary>
        /// Gets or sets the color of the max.
        /// </summary>
        /// <value>The color of the max.</value>
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-04-07    
        /// </remarks>
        Color HigherThanMaxColor { get; set; }

		string UpdatedTimeInUserPerspective { get; }
	}
}