using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Query
{
	public class AllAbsencesHandler : IQueryHandler<AllAbsencesDto, AbsenceDto>
	{
		private readonly IAbsenceRepository _absenceRepository;

		public AllAbsencesHandler(IAbsenceRepository absenceRepository)
		{
			_absenceRepository = absenceRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<AbsenceDto> Handle(AllAbsencesDto query)
		{
			var absences = _absenceRepository.LoadAll();
			return new QueryResultDto<AbsenceDto>
			{
				Successful = true,
				Result = absences.Select(x => new AbsenceDto
				{
					Name = x.Name,
					Priority = x.Priority,
					InPaidTime = x.InPaidTime,
					InWorkTime = x.InWorkTime,
					Confidential = x.Confidential,
					PayrollCode = x.PayrollCode,
					Requestable = x.Requestable
				})
			};
		}
	}
}