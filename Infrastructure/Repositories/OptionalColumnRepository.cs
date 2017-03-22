using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{

    /// <summary>
    /// Represents a OptionalColumnRepository
    /// </summary>
    public class OptionalColumnRepository : Repository<IOptionalColumn>, IOptionalColumnRepository
    {
        public OptionalColumnRepository(IUnitOfWork unitOfWork)
#pragma warning disable 618
            : base(unitOfWork)
#pragma warning restore 618
        {
        }

			public OptionalColumnRepository(ICurrentUnitOfWork currentUnitOfWork)
				: base(currentUnitOfWork)
	    {
		    
	    }

    	/// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IList<IOptionalColumn> GetOptionalColumns<T>()
        {
            ICollection<IOptionalColumn> retList = Session.CreateCriteria(typeof(OptionalColumn))
                        .Add(Restrictions.Eq("TableName", typeof(T).Name))
                        .AddOrder(Order.Asc("Name"))
                        .List<IOptionalColumn>();
            return new List<IOptionalColumn>(retList);
        }

		public IList<IColumnUniqueValues> UniqueValuesOnColumn(Guid column)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
			{
				return ((NHibernateStatelessUnitOfWork)uow).Session.CreateSQLQuery(
					"select distinct Description FROM OptionalColumnValue WHERE Parent =:columnId AND ltrim(description) <> '' ORDER BY description")
					.SetGuid("columnId", column)
					.SetResultTransformer(Transformers.AliasToBean<ColumnUniqueValues>())
					.SetReadOnly(true).List<IColumnUniqueValues>();
			}
		}

	    public IList<IOptionalColumnValue> OptionalColumnValues(IOptionalColumn optionalColumn)
	    {
			ICollection<IOptionalColumnValue> retList = Session.CreateCriteria(typeof(OptionalColumnValue))
				.Add(Restrictions.Eq("Parent", optionalColumn))
				.Add(Restrictions.Not(Restrictions.Eq("Description", string.Empty)))
				.AddOrder(Order.Asc("Description"))
				.List<IOptionalColumnValue>();

			return new List<IOptionalColumnValue>(retList);
		}
    }
	public class ColumnUniqueValues : IColumnUniqueValues
	{
		public string Description { get; set; }
	}
}
