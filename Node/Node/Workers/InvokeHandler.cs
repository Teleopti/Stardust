using System;
using System.Threading;
using Autofac;
using log4net;
using Stardust.Node.Interfaces;

namespace Stardust.Node.Workers
{

    public class InvokeHandler : IInvokeHandler
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(InvokeHandler));
        public InvokeHandler(IComponentContext componentContext)
        {
            if (componentContext == null)
            {
                throw new ArgumentNullException("componentContext");
            }

            ComponentContext = componentContext;
        }

        private IComponentContext ComponentContext { get; set; }

        public void Invoke(object query,
                           CancellationTokenSource cancellationTokenSource,
                           Action<string> progressCallback)
        {
               var handler =
               ComponentContext.Resolve(typeof(IHandle<>).MakeGenericType(query.GetType()));

                var method = handler.GetType()
                    .GetMethod("Handle");


            try //this is to throw right exception and not cause faulted on cancellation
            {
                method.Invoke(handler,
                              new[] {query, cancellationTokenSource, progressCallback});
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