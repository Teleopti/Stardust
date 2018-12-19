using System;
using System.Runtime.Serialization;
using System.Security;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    [Serializable]
    public class ConstraintViolationException : DataSourceException
    {
	    public ConstraintViolationException(string message, Exception inner, string sql)
                                        : base(message, inner)
        {
            Sql = sql;
        }

        public ConstraintViolationException()
        {
        }

        public ConstraintViolationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ConstraintViolationException(string message) : base(message)
        {
        }

        protected ConstraintViolationException(SerializationInfo info,
                                      StreamingContext context)
            : base(info, context)
        {
            Sql = info.GetString("Sql");
        }

        public string Sql { get; }

	    [SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Sql", Sql);
            base.GetObjectData(info, context);
        }
    }
}
