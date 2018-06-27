using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Wfm.Api.Query
{
	public class UserByIdHandler : IQueryHandler<UserByIdDto, UserDto>
	{
		private readonly IPersonRepository _personRepository;

		public UserByIdHandler(IPersonRepository personRepository)
		{
			_personRepository = personRepository;
		}

		[UnitOfWork]
		public virtual QueryResultDto<UserDto> Handle(UserByIdDto command)
		{
			var person = _personRepository.Get(command.PersonId);
			return new QueryResultDto<UserDto>
			{
				Successful = true,
				Result = new []{new UserDto
				{
					FirstName = person.Name.FirstName,
					LastName = person.Name.LastName,
					Email = person.Email,
					Id = person.Id.GetValueOrDefault()
				}}
			};
		}
	}
}