using System;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class GroupingAbsenceCreator
    {
        private readonly IPerson _person;
        private readonly ISessionFactory _sessionFactory;

        public GroupingAbsenceCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        public IGroupingAbsence Create(string name)
        {
            IGroupingAbsence groupingAbsence = new GroupingAbsence(name);

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingAbsence, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingAbsence, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingAbsence, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(groupingAbsence, nu);
            return groupingAbsence;
        }

        public void Save(IGroupingAbsence groupingAbsence)
        {
            ISession sess = _sessionFactory.OpenSession();
            sess.Save(groupingAbsence);
            sess.Flush();
            sess.Close();
        }
    }
}
