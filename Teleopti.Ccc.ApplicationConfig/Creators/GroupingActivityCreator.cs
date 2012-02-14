using System;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class GroupingActivityCreator
    {
        private readonly IPerson _person;
        private readonly ISessionFactory _sessionFactory;

        public GroupingActivityCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        public IGroupingActivity Create(string name)
        {
            IGroupingActivity groupingActivity = new GroupingActivity(name);

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingActivity, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingActivity, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingActivity, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingActivity, nu);
            return groupingActivity;
        }

        public void Save(IGroupingActivity groupingActivity)
        {
            ISession sess = _sessionFactory.OpenSession();
            sess.Save(groupingActivity);
            sess.Flush();
            sess.Close();
        }
    }
}
