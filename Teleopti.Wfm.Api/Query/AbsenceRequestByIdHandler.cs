using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class AbsenceRequestByIdHandler : IQueryHandler<AbsenceRequestByIdDto, AbsenceRequestDto>
	{
		private readonly IPersonRequestRepository _personRequestRepository;
		private readonly ICurrentAuthorization _currentAuthorization;

		public AbsenceRequestByIdHandler(IPersonRequestRepository personRequestRepository, ICurrentAuthorization currentAuthorization)
		{
			_personRequestRepository = personRequestRepository;
			_currentAuthorization = currentAuthorization;
		}

		[UnitOfWork]
		public virtual QueryResultDto<AbsenceRequestDto> Handle(AbsenceRequestByIdDto query)
		{
			var request = _personRequestRepository.Get(query.RequestId);
			if (notExists(request) || notPermitted(request))
			{
				return new QueryResultDto<AbsenceRequestDto> {Successful = false};
			}
			return new QueryResultDto<AbsenceRequestDto>
			{
				Successful = true,
				Result = new []{ new AbsenceRequestDto
				{
					Id = request.Id.GetValueOrDefault(),
					IsNew = request.IsNew,
					IsAlreadyAbsent = request.IsAlreadyAbsent,
					IsApproved = request.IsApproved,
					IsAutoAproved = request.IsAutoAproved,
					IsAutoDenied = request.IsAutoDenied,
					IsCancelled = request.IsCancelled,
					IsDeleted = request.IsDeleted,
					IsDenied = request.IsDenied,
					IsExpired = request.IsExpired,
					IsPending = request.IsPending,
				}}
			};
		}

		private bool notPermitted(IPersonRequest request)
		{
			return !_currentAuthorization.Current().IsPermitted(DefinedRaptorApplicationFunctionPaths.AbsenceRequestsWeb,
				request.Request.Period.StartDateTimeLocal(request.Person.PermissionInformation.DefaultTimeZone()).ToDateOnly(),
				request.Person);
		}

		private static bool notExists(IPersonRequest request)
		{
			return request==null;
		}
	}
}