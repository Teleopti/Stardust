#region Imports

using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

#endregion

namespace Teleopti.Ccc.Infrastructure.Repositories
{

    /// <summary>
    /// Represents a SystemSettingRepository
    /// </summary>
    public class SystemSettingRepository : Repository<ISystemSetting>, 
                                           ISystemSettingRepository
    {

        #region Fields - Instance Member

        #endregion

        #region Properties - Instance Member

        #region Properties - Instance Member - SystemSettingRepository Members

        #endregion

        #endregion

        #region Events - Instance Member

        #endregion

        #region Methods - Instance Member

        #region Methods - Instance Member - SystemSettingRepository Members

        /// <summary>
        /// Initializes a new instance of the <see cref="SystemSettingRepository"/> class.
        /// </summary>
        /// <param name="unitOfWork">The UnitOfWork.</param>
        /// <remarks>
        /// Created by:VirajS
        /// Created date: 2008-10-21
        /// </remarks>
        public SystemSettingRepository(IUnitOfWork unitOfWork) 
            : base(unitOfWork)
        {
            
        }

        /// <summary>
        /// Finds the by setting key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>System setting</returns>
        public ISystemSetting FindBySettingKey(SettingKeys key)
        {
            ISystemSetting systemSeting = null;

            IList<SystemSetting> result = Session.CreateCriteria(typeof (SystemSetting))
                                                .Add(Restrictions.Eq("Name", key))
                                                .List<SystemSetting>();
            if (result != null && result.Count == 1)
                systemSeting = result[0];

            return systemSeting;
        }

        /// <summary>
        /// Finds the by business unit.
        /// </summary>
        /// <param name="businessUnit">The business unit.</param>
        /// <returns>System setting list</returns>
        public IList<ISystemSetting> FindByBusinessUnit(IBusinessUnit businessUnit)
        {
            IList<ISystemSetting> result = Session.CreateCriteria(typeof(SystemSetting))
                                                .Add(Restrictions.Eq("BusinessUnit", businessUnit))
                                                .List<ISystemSetting>();

            return result;
        }

        /// <summary>
        /// Gets the concrete type.
        /// Used when loading one instance by id
        /// </summary>
        protected override Type ConcreteType
        {
            get { return typeof(SystemSetting); }
        }

        #endregion

        #endregion
    }

}
