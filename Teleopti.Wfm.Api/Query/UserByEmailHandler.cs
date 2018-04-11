using System.Linq;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Query
{
	public class UserByEmailHandler : IQueryHandler<UserByEmailDto, UserDto>
	{
		private readonly IPersonRepository _personRepository;

		public UserByEmailHandler(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<UserDto> Handle(UserByEmailDto command)
		{
			var people = _personRepository.FindPeopleByEmail(command.Email);
			return new QueryResultDto<UserDto>
			{
				Successful = true,
				Result = people.Select(p => new UserDto
				{
					FirstName = p.Name.FirstName,
					LastName = p.Name.LastName,
					Email = p.Email,
					Id = p.Id.GetValueOrDefault()
				}).ToArray()
			};
		}
	}
}