using System;
using log4net;

namespace Teleopti.Ccc.Domain.Aop
{
	public class LogManagerWrapper : ILogManagerWrapper
	{
		public ILog GetLogger(Type type)
		{
			return LogManager.GetLogger(type);
		}
	}
}