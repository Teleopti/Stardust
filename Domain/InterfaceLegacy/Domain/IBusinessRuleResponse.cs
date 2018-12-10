using System;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    /// <summary>
    /// Dont know what this is... just extracted the interface
    /// </summary>
    /// <remarks>
    /// Created by: rogerkr
    /// Created date: 2008-08-25
    /// </remarks>
    public interface IBusinessRuleResponse
    {
        /// <summary>
        /// Gets the type of rule.
        /// </summary>
        /// <value>The type of rule.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        Type TypeOfRule { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        /// <value>The message.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks> 
        string Message { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IBusinessRuleResponse"/> is error.
        /// </summary>
        /// <value><c>true</c> if error; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        bool Error { get; }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IBusinessRuleResponse"/> is overriden.
        /// </summary>
        /// <value><c>true</c> if overriden; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        bool Overridden { get; set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IBusinessRuleResponse"/> is mandatory.
        /// </summary>
        /// <value><c>true</c> if mandatory; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-20
        /// </remarks>
        bool Mandatory { get; }

        /// <summary>
        /// Gets the period.
        /// </summary>
        /// <value>The period.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2008-08-22    
        /// /// </remarks>
        DateTimePeriod Period { get; }

		/// <summary>
		/// Gets the date only period.
		/// </summary>
		/// <value>The date only period.</value>
		DateOnlyPeriod DateOnlyPeriod { get; }

        /// <summary>
        /// Gets the person.
        /// </summary>
        /// <value>The person.</value>
        /// /// 
        /// <remarks>
        ///  Created by: Ola
        ///  Created date: 2009-08-31    
        /// /// </remarks>
        IPerson Person { get; }

		string FriendlyName { get; }
    }
}