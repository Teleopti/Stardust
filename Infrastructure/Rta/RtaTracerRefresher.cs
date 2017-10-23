using System;
using Hangfire.Server;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Tracer;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Infrastructure.Rta
{
	public class RtaTracerRefresher : IBackgroundProcess
	{
		private readonly IRtaTracer _tracer;
		private readonly INow _now;
		private DateTime _nextTime;
		private static readonly ILog Log = LogManager.GetLogger(typeof(RtaTracerRefresher));

		public RtaTracerRefresher(IRtaTracer tracer, INow now)
		{
			_tracer = tracer;
			_now = now;
		}

		public void Execute(BackgroundProcessContext context)
		{
			var now = _now.UtcDateTime();
			if (now >= _nextTime)
			{
				try
				{
					_tracer.RefreshTracers();
				}
				catch (Exception e)
				{
					Log.Error("Exception occurred refreshing the rta tracer", e);
				}
				_nextTime = roundUp(now, TimeSpan.FromSeconds(10));
			}
			context.CancellationToken.WaitHandle.WaitOne(TimeSpan.FromMilliseconds(1000));
		}

		private static DateTime roundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
		}
	}
}