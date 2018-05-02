using System;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeActionThrottler : IActionThrottler
	{
		public BlockToken Block(string action)
		{
			throw new NotImplementedException();
		}

		public void Resume(BlockToken blockToken)
		{
			throw new NotImplementedException();
		}

		public void Pause(BlockToken blockToken, TimeSpan allottedPause)
		{
			throw new NotImplementedException();
		}

		public void Finish(BlockToken blockToken)
		{
			throw new NotImplementedException();
		}

		public bool IsBlocked(string action)
		{
			throw new NotImplementedException();
		}
	}
}