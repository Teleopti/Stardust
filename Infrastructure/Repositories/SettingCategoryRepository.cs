using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using Teleopti.Ccc.Domain.SystemSettings;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    /// <summary>
    /// Repository for SettingCategory
    /// </summary>
    /// <remarks>
    /// Created by: henryg
    /// Created date: 2008-07-05
    /// </remarks>
    public class SettingCategoryRepository : Repository<ISettingCategory>, ISettingCategoryRepository
    {
        private const string RootCategoryName = "Root";

        /// <summary>
        /// Initializes a new instance of the <see cref="SettingCategoryRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-07-05
        /// </remarks>
        public SettingCategoryRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
        {
        }

        /// <summary>
        /// Loads the root category.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: HenryG
        /// Created date: 2008-08-21
        /// </remarks>
        public ISettingCategory LoadRootCategory(IPerson person)
        {
            ISettingCategory rootSettingCategory = EagerLoadSettingCategory(person);
            if (rootSettingCategory == null)
            {
                rootSettingCategory = new SettingCategory(RootCategoryName);
                Add(rootSettingCategory);
            }
            return rootSettingCategory;
        }

        /// <summary>
        /// Eagers the load setting category.
        /// </summary>
        /// <param name="person">The person.</param>
        /// <returns></returns>
        /// <remarks>
        /// Created by: henryg
        /// Created date: 2008-08-26
        /// </remarks>
        private ISettingCategory EagerLoadSettingCategory(IPerson person)
        {
            DetachedCriteria categoryCrit = DetachedCriteria.For<SettingCategory>("category");

            DetachedCriteria valueCrit = DetachedCriteria.For<Setting>("setting")
                .CreateAlias("Parent", "category", JoinType.InnerJoin)
                .Add(Restrictions.Or(Restrictions.Eq("Person", person), Restrictions.IsNull("Person")));

            IList res = Session.CreateMultiCriteria()
                .Add(categoryCrit)
                .Add(valueCrit).List();
            ICollection<ISettingCategory> settingCategories = CollectionHelper.ToDistinctGenericCollection<ISettingCategory>(res[0]);

            InitializeGraph(settingCategories);

            return settingCategories.FirstOrDefault(c => c.Name == RootCategoryName);
        }

        private static void InitializeGraph(IEnumerable<ISettingCategory> settingCategories)
        {
            foreach (ISettingCategory pair in settingCategories)
            {
                LazyLoadingManager.Initialize(pair.Values);
                LazyLoadingManager.Initialize(pair.Categories);
            }}

        /// <summary>
        /// Gets the concrete type.
        /// Used when loading one instance by id
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// Created by: rogerkr
        /// Created date: 2008-07-06
        /// </remarks>
        protected override Type ConcreteType
        {
            get { return typeof(SettingCategory); }
        }
    }
}
