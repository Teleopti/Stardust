using System;
using System.Reflection;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    public class SkillTypeCreator
    {
        private readonly IPerson _person;
        private readonly ISessionFactory _sessionFactory;

        public SkillTypeCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _person = person;
            _sessionFactory = sessionFactory;
        }

        public ISkillType Create(Description description, ForecastSource forecastSource)
        {
            ISkillType skillType;
            if (ForecastSource.InboundTelephony == forecastSource)
            {
                skillType = new SkillTypePhone(description, forecastSource);
                
            }
            else
            {
                skillType = new SkillTypeEmail(description, forecastSource);
            }

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(skillType, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(skillType, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(skillType, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(skillType, nu);

            return skillType;
        }

        public bool Save(ISkillType skillType)
        {
            bool skillTypeSaved = false;
            ISession session = _sessionFactory.OpenSession();

            ISkillType foundSkillType = (ISkillType)session.CreateCriteria(typeof (ISkillType))
                .Add(Expression.Eq("Description.Name", skillType.Description.Name))
                .UniqueResult();

            if (foundSkillType == null)
            {
                session.Save(skillType);
                session.Flush();
                skillTypeSaved = true;
            }
            session.Close();
            return skillTypeSaved;
        }

        public ISkillType Fetch(string name)
        {
            ISession session = _sessionFactory.OpenSession();
            
            ISkillType skillType = session.CreateCriteria(typeof(ISkillType))
                        .Add(Expression.Eq("Description.Name", name))
                        .UniqueResult<ISkillType>();
            session.Close();
            return skillType;
        }
    }
}
