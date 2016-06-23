using NHibernate.Envers;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class RevisionListener : IRevisionListener
	{
		private readonly IUpdatedBy _user;

		public RevisionListener(IUpdatedBy user)
		{
			_user = user;
		}

		public void NewRevision(object revisionEntity)
		{
			var revision = (Revision) revisionEntity;
			revision.SetRevisionData(_user.Person());
		}
	}
}