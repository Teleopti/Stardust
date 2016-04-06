using System;
using System.Threading;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public class CurrentInitiatorIdentifier : ICurrentInitiatorIdentifier, IInitiatorIdentifierScope
	{
		private readonly ICurrentUnitOfWork _unitOfWork;
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly ThreadLocal<IInitiatorIdentifier> _threadInitiatorIdentifier = new ThreadLocal<IInitiatorIdentifier>();

		public CurrentInitiatorIdentifier(ICurrentUnitOfWork unitOfWork, ICurrentUnitOfWorkFactory unitOfWorkFactory)
		{
			_unitOfWork = unitOfWork;
			_unitOfWorkFactory = unitOfWorkFactory;
		}

		public IInitiatorIdentifier Current()
		{
			if (_threadInitiatorIdentifier.Value != null)
				return _threadInitiatorIdentifier.Value;
			if (_unitOfWorkFactory.Current().HasCurrentUnitOfWork())
				return _unitOfWork.Current().Initiator();
			return null;
		}

		public IDisposable OnThisThreadUse(IInitiatorIdentifier initiator)
		{
			_threadInitiatorIdentifier.Value = initiator;
			return new GenericDisposable(() =>
			{
				_threadInitiatorIdentifier.Value = null;
			});
		}
	}
}