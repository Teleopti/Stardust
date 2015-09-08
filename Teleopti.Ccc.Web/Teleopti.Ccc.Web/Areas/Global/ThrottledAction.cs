using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Global
{
	internal class ThrottledAction
	{
		public const string Forecasting = "Forecasting";
		public const string Scheduling = "Scheduling";

		private readonly int _limit;
		private readonly IList<Guid> _blocking = new List<Guid>(); 
		
		private readonly static object Lock = new object();

		public ThrottledAction(string action, int limit = 1)
		{
			_limit = limit;
			Action = action;
		}

		public string Action { get; private set; }

		public void Release(Guid token)
		{
			lock (Lock)
			{
				_blocking.Remove(token);
			}
		}

		public Guid AddNew()
		{
			lock (Lock)
			{
				if (isFull()) throw new InvalidOperationException(string.Format("Only {0} actions are allowed concurrently. That limit is currently reached.",_limit));
				var newCaller = Guid.NewGuid();
				_blocking.Add(newCaller);
				return newCaller;
			}
		}

		public bool IsBusy()
		{
			lock (Lock)
			{
				return _blocking.Count >= _limit;
			}
		}

		private bool isFull()
		{
			return _blocking.Count == _limit;
		}
	}
}