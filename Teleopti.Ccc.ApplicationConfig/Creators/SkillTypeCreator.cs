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
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

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

            _setChangeInfoCommand.Execute((AggregateRoot)skillType,_person);

            return skillType;
        }

        public bool Save(ISkillType skillType)
        {
            bool skillTypeSaved = false;
            ISession session = _sessionFactory.OpenSession();

        	var foundSkillType = fetchByName(skillType.Description.Name, session);
            if (foundSkillType == null)
            {
                session.Save(skillType);
                session.Flush();
                skillTypeSaved = true;
            }
            session.Close();
            return skillTypeSaved;
        }

		private static ISkillType fetchByName(string name, ISession session)
		{
			return session.CreateCriteria<SkillType>()
				.Add(Restrictions.Eq("Description.Name", name))
				.UniqueResult<ISkillType>();
		}

        public ISkillType Fetch(string name)
        {
            ISession session = _sessionFactory.OpenSession();

        	var skillType = fetchByName(name, session);
            
            session.Close();
            return skillType;
        }
    }
}
