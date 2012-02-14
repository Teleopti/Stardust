#region Imports

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
    /// Represents a OptionalColumnRepository
    /// </summary>
    public class OptionalColumnRepository : Repository<IOptionalColumn>, IOptionalColumnRepository
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitOfWork"></param>
        public OptionalColumnRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<IOptionalColumn> GetOptionalColumnValues<T>()
        {
            ICollection<IOptionalColumn> retList = Session.CreateCriteria(typeof(OptionalColumn))
                        .Add(Restrictions.Eq("TableName", typeof(T).Name))
                        .AddOrder(Order.Asc("Name"))
                        .List<IOptionalColumn>();
            return new List<IOptionalColumn>(retList);
        }
    }

}
