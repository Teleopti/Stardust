using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;

namespace Teleopti.Ccc.Sdk.Logic.Assemblers
{
	public class PersonCredentialsAppender
	{
		private readonly IAssembler<IPerson, PersonDto> _personAssembler;
		private readonly ITenantPeopleLoader _tenantPeopleLoader;

		public PersonCredentialsAppender(IAssembler<IPerson, PersonDto> personAssembler, ITenantPeopleLoader tenantPeopleLoader)
		{
			_personAssembler = personAssembler;
			_tenantPeopleLoader = tenantPeopleLoader;
		}

		public ICollection<PersonDto> Convert(params IPerson[] persons)
		{
			var personCollection = _personAssembler.DomainEntitiesToDtos(persons).ToList();
			_tenantPeopleLoader.FillDtosWithLogonInfo(personCollection);

			return personCollection;
		}
	}
}