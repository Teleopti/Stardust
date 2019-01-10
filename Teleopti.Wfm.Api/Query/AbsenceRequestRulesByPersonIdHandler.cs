using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Wfm.Api.Query.Request;
using Teleopti.Wfm.Api.Query.Response;

namespace Teleopti.Wfm.Api.Query
{
	public class AbsenceRequestRulesByPersonIdHandler : IQueryHandler<AbsenceRequestRulesByPersonIdDto, AbsenceRequestRuleDto>
	{
		private readonly IAbsenceRepository _absenceRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public AbsenceRequestRulesByPersonIdHandler(IAbsenceRepository absenceRepository, IPersonRepository personRepository, INow now)
		{
			_absenceRepository = absenceRepository;
			_personRepository = personRepository;
			_now = now;
		}

		[UnitOfWork]
		public virtual QueryResultDto<AbsenceRequestRuleDto> Handle(AbsenceRequestRulesByPersonIdDto query)
		{
			var person = _personRepository.Get(query.PersonId);
			if (person?.WorkflowControlSet == null) return new QueryResultDto<AbsenceRequestRuleDto>{Successful = true,Result = new AbsenceRequestRuleDto[0]};

			var period = new DateOnlyPeriod(new DateOnly(query.StartDate), new DateOnly(query.EndDate));
			var absences = _absenceRepository.LoadRequestableAbsence();
			var today = _now.CurrentLocalDate(person.PermissionInformation.DefaultTimeZone());

			return new QueryResultDto<AbsenceRequestRuleDto>
			{
				Successful = true,
				Result = absences.Select(x =>
				{
					var extractor = person.WorkflowControlSet.GetExtractorForAbsence(x);
					extractor.ViewpointDate = today;
					return new AbsenceRequestRuleDto
					{
						AbsenceId = x.Id.GetValueOrDefault(),
						Projection = extractor.Projection.GetProjectedPeriods(period, person.PermissionInformation.Culture(), person.PermissionInformation.UICulture()).Select(y =>
						{
							var projectedPeriod = y.GetPeriod(today);
							return new AbsenceRequestRuleProjectionDto
							{
								StartDate = projectedPeriod.StartDate.Date,
								EndDate = projectedPeriod.EndDate.Date,
								RequestProcess = y.AbsenceRequestProcess.GetType().Name,
								PersonAccountValidator = y.PersonAccountValidator.GetType().Name,
								StaffingThresholdValidator = y.StaffingThresholdValidator.GetType().Name
							};
						}).ToArray()
					};
				})
			};
		}
	}
}