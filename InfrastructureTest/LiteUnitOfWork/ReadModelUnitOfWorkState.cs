using System;
using Teleopti.Ccc.Infrastructure.Web;

namespace Teleopti.Ccc.InfrastructureTest.LiteUnitOfWork
{
	public class ReadModelUnitOfWorkState : ICurrentReadModelUnitOfWork
	{
		[ThreadStatic]
		private static LiteUnitOfWork _unitOfWork;
		private readonly ICurrentHttpContext _httpContext;
		private const string itemsKey = "ReadModelUnitOfWork";

		public ReadModelUnitOfWorkState(ICurrentHttpContext httpContext)
		{
			_httpContext = httpContext;
		}

		public LiteUnitOfWork Get()
		{
			if (_httpContext.Current() != null)
				return (LiteUnitOfWork)_httpContext.Current().Items[itemsKey];
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