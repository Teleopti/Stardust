using log4net;

namespace Teleopti.Ccc.Domain.Aop
{
	public interface ILogManagerWrapper
	{
		ILog GetLogger(string loggerName);
	}
}