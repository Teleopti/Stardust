using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class TimeActivityCreator
    {
        private readonly IPerson _person;
        private readonly IBusinessUnit _businessUnit;
        private readonly ISessionFactory _sessionFactory;

        public TimeActivityCreator(IPerson person, IBusinessUnit businessUnit, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
            _businessUnit = businessUnit;
        }

        public ITimeActivity Create(string name)
        {
            ITimeActivity timeActivity = new TimeActivity {Description = new Description(name)};
            typeof(RemovableAggregateRoot).GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(timeActivity, _person);
            typeof(RemovableAggregateRoot).GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(timeActivity, DateTime.UtcNow);
            typeof(RemovableAggregateRoot)
                .GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(timeActivity, _businessUnit);
            return timeActivity;
        }

        public void Save(ITimeActivity timeActivity)
        {
            ISession sess = _sessionFactory.OpenSession();
            sess.Save(timeActivity);
            sess.Flush();
            sess.Close();
        }
    }
}
