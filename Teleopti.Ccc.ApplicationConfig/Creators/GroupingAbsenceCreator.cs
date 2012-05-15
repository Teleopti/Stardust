using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class GroupingAbsenceCreator
    {
        private readonly IPerson _person;
		private readonly ISessionFactory _sessionFactory;
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

        public GroupingAbsenceCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        public IGroupingAbsence Create(string name)
        {
            IGroupingAbsence groupingAbsence = fetchByName(name);
			if (groupingAbsence == null)
			{
				groupingAbsence = new GroupingAbsence(name);
				_setChangeInfoCommand.Execute((AggregateRoot) groupingAbsence, _person);
			}

        	return groupingAbsence;
        }

		private IGroupingAbsence fetchByName(string name)
		{
			ISession session = _sessionFactory.OpenSession();

			var result = session.CreateCriteria<GroupingAbsence>()
				.Add(Restrictions.Eq("Description.Name", name))
				.List<IGroupingAbsence>();

			session.Close();

			return result.Count == 0 ? null : result[0];
		}

        public void Save(IGroupingAbsence groupingAbsence)
        {
			if (notSavedBefore(groupingAbsence))
			{
				ISession sess = _sessionFactory.OpenSession();
				sess.Save(groupingAbsence);
				sess.Flush();
				sess.Close();
			}
        }

    	private static bool notSavedBefore(IGroupingAbsence groupingAbsence)
    	{
    		return !groupingAbsence.Id.HasValue;
    	}
    }
}
