using System;
using System.Reflection;
using NHibernate;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class CompensationCreator
    {
        private readonly IPerson _person;
        private readonly IBusinessUnit _businessUnit;
        private readonly ISessionFactory _sessionFactory;

        public CompensationCreator(IPerson person, IBusinessUnit businessUnit, ISessionFactory sessionFactory)
        {
            _person = person;
            _businessUnit = businessUnit;
            _sessionFactory = sessionFactory;
        }

        public Compensation Create(string description)
        {
            Compensation compensation = new Compensation();
            compensation.Description = new Description(description);
            typeof(RemovableAggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(compensation, _person);
            typeof(RemovableAggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(compensation, DateTime.UtcNow);
            typeof(RemovableAggregateRoot)
                .GetField("_businessUnit", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(compensation, _businessUnit);
            return compensation;
        }

        public void Save(Compensation compensation)
        {
            ISession session = _sessionFactory.OpenSession();
            session.Save(compensation);
            session.Flush();
            session.Close();
        }
    }
}