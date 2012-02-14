using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
    [Serializable]
    public class CannotConnectToDatabaseException : DataSourceException
    {
        private readonly string _sql;

        public CannotConnectToDatabaseException(string message, Exception inner, string sql)
                                        : base(message, inner)
        {
            _sql = sql;
        }

        public CannotConnectToDatabaseException() : base()
        {
        }

        public CannotConnectToDatabaseException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CannotConnectToDatabaseException(string message) : base(message)
        {
        }

        protected CannotConnectToDatabaseException(SerializationInfo info,
                                      StreamingContext context)
            : base(info, context)
        {
            _sql = info.GetString("Sql");
        }

        public string Sql
        {
            get { return _sql; }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Sql", _sql);
            base.GetObjectData(info, context);
        }
    }
}
