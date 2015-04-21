﻿using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Win.Scheduling.SchedulingScreenInternals
{
	internal class LoaderMethod
	{
		private readonly Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> _action;
		private readonly string _statusStripString;

		public LoaderMethod(Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> action,
							string statusStripString)
		{
			_action = action;
			_statusStripString = statusStripString;
		}

		public Action<IUnitOfWork, ISchedulerStateHolder, Action<ILoaderDeciderResult>, Func<ILoaderDeciderResult>> Action
		{
			get { return _action; }
		}

		public string StatusStripString
		{
			get { return _statusStripString; }
		}
	}
}
