using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate
{
	public class PasswordPolicyForUserMapping : ClassMapping<PasswordPolicyForUser>
	{
		public PasswordPolicyForUserMapping()
		{
			Id("id", mapper => mapper.Generator(new GuidCombGeneratorDef()));
			ManyToOne(x => x.PersonInfo, mapper => mapper.Column("Person"));
			Property(x=>x.LastPasswordChange);
			Property(x=>x.InvalidAttemptsSequenceStart);
			Property(x=>x.InvalidAttempts);
			Property(x=>x.IsLocked);

			Table("UserDetail");
		}
	}
}