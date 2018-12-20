using System;
using System.Runtime.Serialization;
using System.Security;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Infrastructure
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
	        DataSource = dataSourceName();
		}

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public DataSourceException(string message)
            : base(message)
        {
	        DataSource = dataSourceName();
		}


        /// <summary>
        /// Initializes a new instance of the <see cref="DataSourceException"/> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public DataSourceException(string message, Exception innerException)
            : base(message, innerException)
        {
	        DataSource = dataSourceName();
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
	        DataSource = info.GetString("DataSource");
        }

		public override string Message => $"{base.Message} - (Tenant: {DataSource})";

		public string DataSource { get; }

	    private string dataSourceName()
	    {
		    var dataSource = new DataSourceState().Get();
		    if (dataSource != null)
		    {
			    return dataSource.DataSourceName;
		    }

			var principal = GetCurrentPrincipal();
		    var identity = principal?.Identity as ITeleoptiIdentity;
		    if (identity == null)
			    return "[unknown datasource]";
		    return identity.DataSource.DataSourceName;
	    }
		
        protected virtual ITeleoptiPrincipal GetCurrentPrincipal()
        {
            return TeleoptiPrincipalForLegacy.CurrentPrincipal;
        }

	    [SecurityCritical]
	    public override void GetObjectData(SerializationInfo info, StreamingContext context)
	    {
		    info.AddValue("DataSource", DataSource);
		    base.GetObjectData(info, context);
	    }
    }
}