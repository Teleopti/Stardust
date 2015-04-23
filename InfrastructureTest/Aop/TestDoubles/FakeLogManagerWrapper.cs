using System;
using log4net;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.InfrastructureTest.Aop.TestDoubles
{
	public class FakeLogManagerWrapper : ILogManagerWrapper
	{
		private readonly ILog _logger;

		public FakeLogManagerWrapper(ILog logger)
		{
			_logger = logger;
		}

		public ILog GetLogger(Type type)
		{
			return _logger;
		}
	}
}