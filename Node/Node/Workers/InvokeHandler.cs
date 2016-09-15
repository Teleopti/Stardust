﻿using System;
using System.Threading;
using Autofac;
using log4net;
using Stardust.Node.Extensions;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{
	public class InvokeHandler : IInvokeHandler
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof (InvokeHandler));

		public InvokeHandler(ILifetimeScope componentContext)
		{
			if (componentContext == null)
			{
				throw new ArgumentNullException("componentContext");
			}

			ComponentContext = componentContext;
		}

		private ILifetimeScope ComponentContext { get; set; }

		public void Invoke(object query,
		                   CancellationTokenSource cancellationTokenSource,
		                   Action<string> progressCallback)
		{
			using (var lifetimeScope = ComponentContext.BeginLifetimeScope())
			{
				var handler =
					lifetimeScope.Resolve(typeof(IHandle<>).MakeGenericType(query.GetType()));

				if (handler == null)
				{
					Logger.ErrorWithLineNumber(string.Format("The job type [{0}] could not be resolved. The job cannot be started.",
					                                         query.GetType()));

					throw new Exception("The handler " + query.GetType() + " could not be resolved");
				}

				var method = handler.GetType().GetMethod("Handle");

				if (method == null)
				{
					Logger.ErrorWithLineNumber(string.Format("The method for handler [{0}] could not be found. ",
					                                         handler.GetType()));

					throw new Exception("The method 'Handle' for handler " + handler.GetType() + " could not be found");
				}

				//this is to throw right exception and not cause faulted on cancellation
				try
				{
					method.Invoke(handler,
					              new[]
					              {
						              query,
						              cancellationTokenSource,
						              progressCallback
					              });
				}
				catch (Exception ex)
				{
					if (ex.InnerException is OperationCanceledException)
					{
						throw ex.InnerException;
					}
					throw;
				}
			}
		}
	}
}