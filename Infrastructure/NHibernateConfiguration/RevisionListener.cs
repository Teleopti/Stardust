using System;
using NHibernate.Envers;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.NHibernateConfiguration
{
	public class RevisionListener : IRevisionListener
	{
		private readonly IUnsafePersonProvider _personProvider;

		public RevisionListener(IUnsafePersonProvider personProvider)
		{
			_personProvider = personProvider;
		}

		public void NewRevision(object revisionEntity)
		{
			var revision = (Revision) revisionEntity;
			revision.SetRevisionData(_personProvider.CurrentUser());
		}
	}
}