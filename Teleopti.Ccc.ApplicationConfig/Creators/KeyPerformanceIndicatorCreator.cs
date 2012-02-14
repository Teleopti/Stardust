using System;
using System.Drawing;
using System.Reflection;
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
        private IPerson _person;

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
            KeyPerformanceIndicator keyPerformanceIndicator = new KeyPerformanceIndicator(name, resourceKey, targetValueType, defaultTargetValue, defaultMinValue, defaultMaxValue, defaultBetweenColor, defaultLowerThanMinColor, defaultHigherThanMaxColor) ;

            DateTime nu = DateTime.Now;
            typeof(AggregateRoot)
                .GetField("_createdBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(keyPerformanceIndicator, _person);
            typeof(AggregateRoot)
                .GetField("_createdOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(keyPerformanceIndicator, nu);
            typeof(AggregateRoot)
                .GetField("_updatedBy", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(keyPerformanceIndicator, _person);
            typeof(AggregateRoot)
                .GetField("_updatedOn", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(keyPerformanceIndicator, nu);

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

            KeyPerformanceIndicator foundKeyPerformanceIndicator = (KeyPerformanceIndicator)session.CreateCriteria(typeof(KeyPerformanceIndicator))
                .Add(Expression.Eq("ResourceKey", keyPerformanceIndicator.ResourceKey))
                .UniqueResult();

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

            KeyPerformanceIndicator keyPerformanceIndicator = session.CreateCriteria(typeof(KeyPerformanceIndicator))
                        .Add(Expression.Eq("ResourceKey", name))
                        .UniqueResult<KeyPerformanceIndicator>();
            session.Close();
            return keyPerformanceIndicator;
        }
    }
}
