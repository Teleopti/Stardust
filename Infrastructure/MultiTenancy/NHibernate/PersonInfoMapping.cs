using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class PersonInfoMapping : ClassMapping<PersonInfo>
	{
		public PersonInfoMapping()
		{
			Id("Id", mapper => mapper.Generator(new GuidCombGeneratorDef()));
			Property("terminalDate",mapper => mapper.Access(Accessor.Field));
			Property("isDeleted", mapper => mapper.Access(Accessor.Field));
			Property("terminalDate", mapper => mapper.Access(Accessor.Field));

			Join("ApplicationAuthenticationInfo", mapper => {
				mapper.Key(keyMapper => keyMapper.Column("Person"));
				mapper.Fetch(FetchKind.Join);
				mapper.Optional(true);
				mapper.Table("ApplicationAuthenticationInfo");
				mapper.Property(x => x.Password);
				mapper.Property("applicationLogonName", m => m.Access(Accessor.Field));
				});

			Table("Person");
		}
	}
}