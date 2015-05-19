using System;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.Infrastructure.LiteUnitOfWork.MessageBrokerUnitOfWork
{
	public class MessageBrokerUnitOfWorkState : ICurrentMessageBrokerUnitOfWork
	{
		[ThreadStatic] 
		private static LiteUnitOfWork _unitOfWork;
		private readonly ICurrentHttpContext _httpContext;
		private const string itemsKey = "MessageBrokerUnitOfWork";

		public MessageBrokerUnitOfWorkState(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public LiteUnitOfWork Get()
		{
			if (_httpContext.Current() != null)
				return (LiteUnitOfWork) _httpContext.Current().Items[itemsKey];
			return _unitOfWork;
		}

		public void Set(LiteUnitOfWork uow)
		{
			if (_httpContext.Current() != null)
			{
				_httpContext.Current().Items[itemsKey] = uow;
				return;
			}
			_unitOfWork = uow;
		}

		public ILiteUnitOfWork Current()
		{
			return Get();
		}
	}
}