using System;
using Hangfire.Server;
using log4net;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Tracer;

namespace Teleopti.Ccc.Infrastructure.RealTimeAdherence.Tracer
{
	public class RtaTracerRefresher : IBackgroundProcess
	{
		private readonly IRtaTracer _tracer;
		private readonly INow _now;
		private readonly TimeSpan _interval = TimeSpan.FromSeconds(10);

		private static readonly ILog Log = LogManager.GetLogger(typeof(RtaTracerRefresher));

		public RtaTracerRefresher(IRtaTracer tracer, INow now)
		{
			_tracer = tracer;
			_now = now;
		}

		public void Execute(BackgroundProcessContext context)
		{
			try
			{
				_tracer.RefreshTracers();
				_tracer.FlushBuffer();
				_tracer.PurgeLogs();
				context.CancellationToken.WaitHandle.WaitOne(waitTime());
			}
			catch (Exception e)
			{
				Log.Error("Exception occurred refreshing the rta tracer", e);
				context.CancellationToken.WaitHandle.WaitOne(_interval);
			}
		}

		private TimeSpan waitTime()
		{
			var nextTime = roundUp(_now.UtcDateTime().AddSeconds(1), _interval);
			var untilNextTime = _now.UtcDateTime().Subtract(nextTime).Duration();
			return untilNextTime > _interval ? _interval : untilNextTime;
		}

		private static DateTime roundUp(DateTime dt, TimeSpan d)
		{
			return new DateTime(((dt.Ticks + d.Ticks - 1) / d.Ticks) * d.Ticks);
		}
	}
}