using System;
using System.Runtime.Serialization;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    /// <summary>
    /// Used when exception occurs to and from datasource
    /// </summary>
    [Serializable]
    public class DataSourceException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        public DataSourceException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DataSourceException(string message)
            : base(message)
        {
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataSourceException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext"></see> that contains contextual information about the source or destination.</param>
        /// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult"></see> is zero (0). </exception>
        /// <exception cref="T:System.ArgumentNullException">The info parameter is null. </exception>
        protected DataSourceException(SerializationInfo info,
                                      StreamingContext context) : base(info, context)
        {
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public string DataSource
        {
            get
            {
                var principal = GetCurrentPrincipal();
                if (principal == null)
                    return "[unknown datasource]";
                var identity = principal.Identity as ITeleoptiIdentity;
                if (identity == null)
                    return "[unknown datasource]";
                return identity.DataSource.DataSourceName;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        protected virtual ITeleoptiPrincipal GetCurrentPrincipal()
        {
            return TeleoptiPrincipal.CurrentPrincipal;
        }
    }
}