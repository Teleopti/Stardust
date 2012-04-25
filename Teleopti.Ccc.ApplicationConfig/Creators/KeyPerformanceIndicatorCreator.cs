﻿using System.Drawing;
using NHibernate;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.ApplicationConfig.Creators
{
    /// <summary>
    /// Creates the default KpiPerformanceInicators
    /// </summary>
    /// <remarks>
    /// Created by: zoet
    /// Created date: 2009-04-27
    /// </remarks>
   public class KeyPerformanceIndicatorCreator
    {
        private readonly ISessionFactory _sessionFactory;
		private readonly IPerson _person;
		private readonly SetChangeInfoCommand _setChangeInfoCommand = new SetChangeInfoCommand();

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyPerformanceIndicatorCreator"/> class.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <param name="sessionFactory">The session factory.</param>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-27
        /// </remarks>
        public KeyPerformanceIndicatorCreator(IPerson person, ISessionFactory sessionFactory)
        {
            _sessionFactory = sessionFactory;
            _person = person;
        }

        /// <summary>
        /// Creates the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="resourceKey">The resource key.</param>
        /// <param name="targetValueType">Type of the target value.</param>
        /// <param name="defaultTargetValue">The default target value.</param>
        /// <param name="defaultMinValue">The default min value.</param>
        /// <param name="defaultMaxValue">The default max value.</param>
        /// <param name="defaultBetweenColor">Color of the default between.</param>
        /// <param name="defaultLowerThanMinColor">Color of the default lower than min.</param>
        /// <param name="defaultHigherThanMaxColor">Color of the default higher than max.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-27
        /// </remarks>
        public KeyPerformanceIndicator Create(string name, string resourceKey, EnumTargetValueType targetValueType, int defaultTargetValue, int defaultMinValue, int defaultMaxValue, Color defaultBetweenColor, Color defaultLowerThanMinColor, Color defaultHigherThanMaxColor)
        {
            var keyPerformanceIndicator = new KeyPerformanceIndicator(name, resourceKey, targetValueType, defaultTargetValue, defaultMinValue, defaultMaxValue, defaultBetweenColor, defaultLowerThanMinColor, defaultHigherThanMaxColor) ;

            _setChangeInfoCommand.Execute((AggregateRoot)keyPerformanceIndicator,_person);

            return keyPerformanceIndicator;
        }

        /// <summary>
        /// Saves the specified key performance indicator.
        /// </summary>
        /// <param name="keyPerformanceIndicator">The key performance indicator.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-27
        /// </remarks>
        public bool Save(KeyPerformanceIndicator keyPerformanceIndicator)
        {
            bool keyPerformanceIndicatorSaved = false;
            ISession session = _sessionFactory.OpenSession();

			var foundKeyPerformanceIndicator = session.CreateCriteria<KeyPerformanceIndicator>()
                .Add(Restrictions.Eq("ResourceKey", keyPerformanceIndicator.ResourceKey))
				.UniqueResult<KeyPerformanceIndicator>();

            if (foundKeyPerformanceIndicator == null)
            {
                session.Save(keyPerformanceIndicator);
                session.Flush();
                keyPerformanceIndicatorSaved = true;
            }
            session.Close();
            return keyPerformanceIndicatorSaved;
        }

        /// <summary>
        /// Fetches the specified name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: zoet
        /// Created date: 2009-04-27
        /// </remarks>
        public KeyPerformanceIndicator Fetch(string name)
        {
            ISession session = _sessionFactory.OpenSession();

            var keyPerformanceIndicator = session.CreateCriteria<KeyPerformanceIndicator>()
                        .Add(Restrictions.Eq("ResourceKey", name))
                        .UniqueResult<KeyPerformanceIndicator>();

            session.Close();
            return keyPerformanceIndicator;
        }
    }
}
