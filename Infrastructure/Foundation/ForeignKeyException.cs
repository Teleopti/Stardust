using System;
using System.Runtime.Serialization;
using System.Security;
using Teleopti.Ccc.Domain.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	[Serializable]
	public class ForeignKeyException : DataSourceException
	{
		public ForeignKeyException(string message, Exception inner, string sql)
                                        : base(message, inner)
        {
            Sql = sql;
        }

        public ForeignKeyException()
        {
        }

        public ForeignKeyException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public ForeignKeyException(string message) : base(message)
        {
        }

		protected ForeignKeyException(SerializationInfo info,
                                      StreamingContext context)
            : base(info, context)
        {
            Sql = info.GetString("Sql");
        }

		public string Sql { get; private set; }

		[SecurityCritical]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Sql", Sql);
            base.GetObjectData(info, context);
        }
	}
}