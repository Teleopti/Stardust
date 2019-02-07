using log4net;

namespace Teleopti.Ccc.Domain.Aop
{
	public interface ILogManager
	{
		ILog GetLogger(string loggerName);
	}
}