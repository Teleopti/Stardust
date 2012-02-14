using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Interface for general datahandling on a form.
    /// </summary>
    /// <remarks>
    /// Created by: tamasb
    /// Created date: 12/10/2007
    /// </remarks>
    public interface IGuestDataHandling<T> : IDataExchange
    {
        /// <summary>
        /// Gets or sets the datasource.
        /// </summary>
        /// <value>The data source.</value>
        /// <remarks>
        /// Created by: tamasb
        /// Created date: 12/10/2007
        /// </remarks>
        T DataSource{get;set;}

    }
}
