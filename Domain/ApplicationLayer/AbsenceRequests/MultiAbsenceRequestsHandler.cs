using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class MultiAbsenceRequestsHandler : IHandleEvent<NewMultiAbsenceRequestsCreatedEvent>, IRunOnStardust
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(MultiAbsenceRequestsHandler));
		private readonly IPersonRequestRepository _personRequestRepository;
		private List<IPersonRequest> _personRequests;
		private readonly IMultiAbsenceRequestProcessor _absenceRequestProcessor;
		private static readonly isNullOrNotNewSpecification personRequestSpecification = new isNullOrNotNewSpecification();
		private static readonly isNullSpecification absenceRequestSpecification = new isNullSpecification();
		private readonly ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private readonly IStardustJobFeedback _feedback;



		public MultiAbsenceRequestsHandler(IPersonRequestRepository personRequestRepository,
			IMultiAbsenceRequestProcessor absenceRequestProcessor, ICurrentUnitOfWorkFactory currentUnitOfWorkFactory, IStardustJobFeedback stardustJobFeedback)
		{
			_personRequestRepository = personRequestRepository;
			_absenceRequestProcessor = absenceRequestProcessor;
			_currentUnitOfWorkFactory = currentUnitOfWorkFactory;
			_feedback = stardustJobFeedback;

			if (logger.IsInfoEnabled)
			{
				logger.Info("New instance of handler was created");
			}
		}

		[AsSystem]
		public virtual void Handle(NewMultiAbsenceRequestsCreatedEvent @event)
		{
			checkPersonRequest(@event.PersonRequestIds);
			_feedback.SendProgress?.Invoke("Done Checking Person Requests.");
			if (!_personRequests.IsNullOrEmpty())
				_absenceRequestProcessor.ProcessAbsenceRequest(_personRequests);
		}


		private void checkPersonRequest(List<Guid> personRequestIds)
		{
			using (var uow =_currentUnitOfWorkFactory.Current().CreateAndOpenUnitOfWork())
			{
				_personRequests = new List<IPersonRequest>();

				
				var personRequests = _personRequestRepository.Find(personRequestIds);
				DateTime min = DateTime.MaxValue;
				DateTime max = DateTime.MinValue;
				foreach (var personRequest in personRequests)
				{
					if (personRequestSpecification.IsSatisfiedBy(personRequest))
					{
						if (logger.IsWarnEnabled)
						{
							logger.WarnFormat(
								"No person request found with the supplied Id, or the request is not in New status mode. (Id = {0})",
								personRequest.Id);
						}
					}
					else if (absenceRequestSpecification.IsSatisfiedBy((IAbsenceRequest) personRequest.Request))
					{
						if (logger.IsWarnEnabled)
						{
							logger.WarnFormat("The found person request is not of type absence request. (Id = {0})",
											  personRequest.Id);
						}
					}
					else
					{
						personRequest.Pending();
						_personRequests.Add(personRequest);
					}
						
				}
				if (_personRequests.Any())
				{
					min = _personRequests.Min(x => x.Request.Period.StartDateTime);
					max = _personRequests.Max(x => x.Request.Period.EndDateTime);
				}
				
				if (max > min)
				{
					DateTimePeriod period = new DateTimePeriod(min, max);
					var waitListIds = _personRequestRepository.GetWaitlistRequests(period).ToList();
					_personRequests.AddRange(_personRequestRepository.Find(waitListIds));
				}

				uow.PersistAll();
			}
		}

		private class isNullOrNotNewSpecification : Specification<IPersonRequest>
		{
			public override bool IsSatisfiedBy(IPersonRequest obj)
			{
				return (obj == null || !obj.IsNew);
			}
		}

		private class isNullSpecification : Specification<IAbsenceRequest>
		{
			public override bool IsSatisfiedBy(IAbsenceRequest obj)
			{
				return (obj == null);
			}
		}
	}
}
