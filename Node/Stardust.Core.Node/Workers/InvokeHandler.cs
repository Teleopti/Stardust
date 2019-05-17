using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Stardust.Core.Node.Extensions;
using Stardust.Core.Node.Interfaces;
//using Autofac;

//using log4net;

namespace Stardust.Core.Node.Workers
{
	public class InvokeHandler : IInvokeHandler
	{
		private readonly ILogger Logger = new LoggerFactory().CreateLogger(typeof (InvokeHandler));

		public InvokeHandler(IServiceProvider componentContext)
		{
			if (componentContext == null)
			{
				throw new ArgumentNullException(nameof(componentContext));
			}

			ComponentContext = componentContext;
		}

		private IServiceProvider ComponentContext { get; }

		public void Invoke(object query, 
			CancellationTokenSource cancellationTokenSource, 
			Action<string> progressCallback)
		{
			//using (var lifetimeScope = ComponentContext.GetService<IHandle<>>())
			{

                //           var handler =
                //lifetimeScope.Resolve(typeof(IHandle<>).MakeGenericType(query.GetType()));
                var handler =
                  ComponentContext.GetService(typeof(IHandle<>).MakeGenericType(query.GetType()));


                if (handler == null)
				{
					Logger.ErrorWithLineNumber($"The job type [{query.GetType()}] could not be resolved. The job cannot be started.");

					throw new Exception("The handler " + query.GetType() + " could not be resolved");
				}

				var method = handler.GetType().GetMethod("Handle");

				if (method == null)
				{
					Logger.ErrorWithLineNumber($"The method for handler [{handler.GetType()}] could not be found. ");

					throw new Exception("The method 'Handle' for handler " + handler.GetType() + " could not be found");
				}

				//this is to throw right exception and not cause faulted on cancellation
				try
				{
					var arguments = new []{ query, cancellationTokenSource, progressCallback, null };

					method.Invoke(handler,
					              arguments);

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