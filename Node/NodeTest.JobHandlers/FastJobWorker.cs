﻿using System;
using System.Threading;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Helpers;
using Stardust.Node.Interfaces;
using Stardust.Node.Log4Net;
using Stardust.Node.Log4Net.Extensions;

namespace NodeTest.JobHandlers
{
	public class FastJobWorker : IHandle<FastJobParams>

	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (FastJobWorker));

		private readonly FastJobCode _fastJobCode;

		public FastJobWorker(FastJobCode fastJobCode)
		{
			_fastJobCode = fastJobCode;

			Logger.DebugWithLineNumber("'Fast Job Worker' class constructor called.");
		}

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public void Handle(FastJobParams parameters,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> sendProgress)
		{
			Logger.DebugWithLineNumber("'Fast Job Worker' handle method called.");

			CancellationTokenSource = cancellationTokenSource;

			_fastJobCode.DoTheThing(parameters,
			                        cancellationTokenSource,
			                        sendProgress);
		}
	}
}