using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Domain.Common
{
	[Serializable]
	public class BusinessRuleValidationException : Exception
	{

		public BusinessRuleValidationException()
		{
		}

		public BusinessRuleValidationException(string message)
			: base(message)
		{
		}


		public BusinessRuleValidationException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		protected BusinessRuleValidationException(SerializationInfo info,
			StreamingContext context)
			: base(info, context)
		{
		}
	}
}