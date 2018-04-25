using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.Web.WebInteractions;

namespace Teleopti.Ccc.Intraday.PerformanceTest
{
	public class IntradayPerformanceTestAttribute : IoCTestAttribute
	{
		protected override void Extend(IExtend extend, IIocConfiguration configuration)
		{
			base.Extend(extend, configuration);
			extend.AddService<Http>();
			extend.AddService<TimeSetter>();
		}
	}
}