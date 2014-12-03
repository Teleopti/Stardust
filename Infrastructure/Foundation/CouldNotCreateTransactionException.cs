using System;
using System.Runtime.Serialization;
using System.Security;

namespace Teleopti.Ccc.Infrastructure.Foundation
{
	[Serializable]
	public class CouldNotCreateTransactionException : DataSourceException
	{
		public CouldNotCreateTransactionException(string message, Exception inner, string sql)
                                        : base(message, inner)
        {
            Sql = sql;
        }

        public CouldNotCreateTransactionException()
        {
        }

        public CouldNotCreateTransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public CouldNotCreateTransactionException(string message) : base(message)
        {
        }

		protected CouldNotCreateTransactionException(SerializationInfo info,
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
