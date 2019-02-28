using System;
using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Infrastructure.Repositories
{

    /// <summary>
    /// Represents a OptionalColumnRepository
    /// </summary>
    public class OptionalColumnRepository : Repository<IOptionalColumn>, IOptionalColumnRepository
    {
		public static OptionalColumnRepository DONT_USE_CTOR(ICurrentUnitOfWork currentUnitOfWork)
		{
			return new OptionalColumnRepository(currentUnitOfWork, null, null);
		}

		public static OptionalColumnRepository DONT_USE_CTOR(IUnitOfWork unitOfWork)
		{
			return new OptionalColumnRepository(new ThisUnitOfWork(unitOfWork), null, null);
		}

		public OptionalColumnRepository(ICurrentUnitOfWork currentUnitOfWork, ICurrentBusinessUnit currentBusinessUnit, Lazy<IUpdatedBy> updatedBy)
			: base(currentUnitOfWork, currentBusinessUnit, updatedBy)
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
				return uow.Session().CreateSQLQuery(
					"select distinct Description FROM OptionalColumnValue WHERE ReferenceId =:columnId AND ltrim(description) <> '' ORDER BY description")
					.SetGuid("columnId", column)
					.SetResultTransformer(Transformers.AliasToBean<ColumnUniqueValues>())
					.SetReadOnly(true).List<IColumnUniqueValues>();
			}
		}
		public IList<IColumnUniqueValues> UniqueValuesOnColumnWithValidPerson(Guid column)
		{
			using (var uow = UnitOfWorkFactory.Current.CreateAndOpenStatelessUnitOfWork())
			{
				return uow.Session().CreateSQLQuery(
					"SELECT DISTINCT Description FROM OptionalColumnValue ocv " +
					"INNER JOIN Person WITH (NOLOCK) ON Person.Id = ocv.Parent  AND Person.IsDeleted = 0 " +
					"WHERE ocv.ReferenceId =:columnId AND ltrim(ocv.Description) <> '' " +
					"ORDER BY ocv.Description")
					.SetGuid("columnId", column)
					.SetResultTransformer(Transformers.AliasToBean<ColumnUniqueValues>())
					.SetReadOnly(true).List<IColumnUniqueValues>();
			}
		}

	    public IList<IOptionalColumnValue> OptionalColumnValues(IOptionalColumn optionalColumn)
	    {
			ICollection<IOptionalColumnValue> retList = Session.CreateCriteria(typeof(OptionalColumnValue))
				.Add(Restrictions.Eq("ReferenceObject", optionalColumn))
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
