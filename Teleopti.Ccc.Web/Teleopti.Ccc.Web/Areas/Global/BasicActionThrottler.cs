using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Teleopti.Ccc.Domain.Common;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class BasicActionThrottler : IActionThrottler
	{
		private CancellationTokenSource _cancelPauseAction;
		private readonly IList<ThrottledAction> _actions = new[] {new ThrottledAction(ThrottledAction.Forecasting), new ThrottledAction(ThrottledAction.Scheduling)};

		public BlockToken Block(string action)
		{
			var throttled = findAction(action);

			return new BlockToken {Action = action, Token = throttled.AddNew()};
		}

		public void Resume(BlockToken blockToken)
		{
			if (_cancelPauseAction != null)
			{
				_cancelPauseAction.Cancel();
			}
		}

		public void Pause(BlockToken blockToken, TimeSpan allottedPause)
		{
			_cancelPauseAction = new CancellationTokenSource();
			Task.Delay(allottedPause, _cancelPauseAction.Token).ContinueWith(_ => Finish(blockToken), _cancelPauseAction.Token);
		}

		public void Finish(BlockToken blockToken)
		{
			var throttled = findAction(blockToken.Action);
			throttled.Release(blockToken.Token);
		}

		private ThrottledAction findAction(string action)
		{
			var throttled = _actions.FirstOrDefault(a => a.Action == action);
			if (throttled == null) throw new InvalidOperationException("This action is not allowed to be blocked.");
			return throttled;
		}

		public bool IsBlocked(string action)
		{
			var throttled = findAction(action);
			return throttled.IsBusy();
		}
	}
}