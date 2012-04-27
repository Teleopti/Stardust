using Autofac;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;

namespace Teleopti.Ccc.Sdk.WcfService
{
    public class InvokeQuery<TResult> : IInvokeQuery<TResult>
    {
        private readonly ILifetimeScope _lifetimeScope;

        public InvokeQuery(ILifetimeScope lifetimeScope)
        {
            _lifetimeScope = lifetimeScope;
        }

        public TResult Invoke(QueryDto query)
        {
            var handler = _lifetimeScope.Resolve(typeof(IHandleQuery<,>).MakeGenericType(new[] { query.GetType(), typeof(TResult) }));
            var method = handler.GetType().GetMethod("Handle");
            var result = method.Invoke(handler, new [] {query});
            return (TResult) result;
        }
    }
}