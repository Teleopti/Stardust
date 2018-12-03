using NHibernate;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Ccc.Infrastructure.UnitOfWork
{
    public class NHibernateStatelessUnitOfWork : IStatelessUnitOfWork
    {
		protected internal NHibernateStatelessUnitOfWork(IStatelessSession session)
        {
            InParameter.NotNull(nameof(session), session);
            Session = session;
        }

		protected internal IStatelessSession Session { get; }

        public void Dispose()
        {
			Session?.Dispose();
        }
    }
}
