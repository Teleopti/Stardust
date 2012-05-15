using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class GroupingActivityCreator
    {
        private readonly IPerson _person;
		private readonly ISessionFactory _sessionFactory;
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

        public GroupingActivityCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

		public IGroupingActivity Create(string name)
		{
			IGroupingActivity groupingActivity = fetchByName(name);
			if (groupingActivity == null)
			{
				groupingActivity = new GroupingActivity(name);
				_setChangeInfoCommand.Execute((AggregateRoot)groupingActivity, _person);
			}

			return groupingActivity;
		}

		private IGroupingActivity fetchByName(string name)
		{
			ISession session = _sessionFactory.OpenSession();

			var result = session.CreateCriteria<GroupingActivity>()
				.Add(Restrictions.Eq("Description.Name", name))
				.List<IGroupingActivity>();

			session.Close();

			return result.Count == 0 ? null : result[0];
		}

		public void Save(IGroupingActivity groupingActivity)
		{
			if (notSavedBefore(groupingActivity))
			{
				ISession sess = _sessionFactory.OpenSession();
				sess.Save(groupingActivity);
				sess.Flush();
				sess.Close();
			}
		}

		private static bool notSavedBefore(IGroupingActivity groupingActivity)
		{
			return !groupingActivity.Id.HasValue;
		}
    }
}
