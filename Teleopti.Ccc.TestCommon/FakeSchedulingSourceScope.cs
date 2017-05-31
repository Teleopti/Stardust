using System;
using Teleopti.Ccc.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon
{
	public class FakeSchedulingSourceScope : ICurrentSchedulingSource, ISchedulingSourceScope
	{
		public string SchedulingSource { get; set; }

		public string Current()
		{
			return SchedulingSource;
		}

		public IDisposable OnThisThreadUse(string schedulingScope)
		{
			SchedulingSource = schedulingScope;
			return new GenericDisposable(() => {});
		}
	}
}