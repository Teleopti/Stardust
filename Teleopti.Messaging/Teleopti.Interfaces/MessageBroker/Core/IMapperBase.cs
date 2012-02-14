using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Teleopti.Interfaces.MessageBroker.Core
{
    /// <summary>
    /// The mapper base.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// Created by: ankarlp
    /// Created date: 2008-08-07
    /// </remarks>
    public interface IMapperBase<T>
    {
        /// <summary>
        /// Maps all.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        IList<T> MapAll(IDataReader reader);
        /// <summary>
        /// Inserts all.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        bool InsertAll(ICollection<T> collection);
        /// <summary>
        /// Inserts the specified connection.
        /// </summary>
        /// <param name="connection">The connection.</param>
        /// <param name="domainObject">The domain object.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: ankarlp
        /// Created date: 2008-08-07
        /// </remarks>
        bool Insert(SqlConnection connection, T domainObject);
    }
}