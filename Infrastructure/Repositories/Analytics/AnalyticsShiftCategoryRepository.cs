using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Infrastructure.Repositories.Analytics
{
	public class AnalyticsShiftCategoryRepository : IAnalyticsShiftCategoryRepository
	{
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;

		public AnalyticsShiftCategoryRepository(ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork)
		{
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
		}

		public IList<AnalyticsShiftCategory> ShiftCategories()
		{
			return _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"select [shift_category_id] ShiftCategoryId
					,[shift_category_code] ShiftCategoryCode
					,[shift_category_name] ShiftCategoryName
					,[shift_category_shortname] ShiftCategoryShortname
					,[display_color] DisplayColor
					,[business_unit_id] BusinessUnitId
					,[datasource_id] DatasourceId
					,[insert_date] InsertDate
					,[update_date] UpdateDate
					,[datasource_update_date] DatasourceUpdateDate
					,[is_deleted] IsDeleted
					from [mart].[dim_shift_category] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsShiftCategory)))
				.SetReadOnly(true)
				.List<AnalyticsShiftCategory>();
		}

		public void AddShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_shift_category_insert]
						@shift_category_code=:ShiftCategoryCode, 
						@shift_category_name=:ShiftCategoryName, 
						@shift_category_shortname=:ShiftCategoryShortname, 
						@display_color=:DisplayColor, 
						@business_unit_id=:BusinessUnitId,
						@datasource_id=:DatasourceId,
						@datasource_update_date=:DatasourceUpdateDate,
						@is_deleted=:IsDeleted
					  ")
				.SetGuid("ShiftCategoryCode", analyticsShiftCategory.ShiftCategoryCode)
				.SetString("ShiftCategoryName", analyticsShiftCategory.ShiftCategoryName)
				.SetString("ShiftCategoryShortname", analyticsShiftCategory.ShiftCategoryShortname)
				.SetInt32("DisplayColor", analyticsShiftCategory.DisplayColor)
				.SetInt32("BusinessUnitId", analyticsShiftCategory.BusinessUnitId)
				.SetInt32("DatasourceId", analyticsShiftCategory.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", analyticsShiftCategory.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", analyticsShiftCategory.IsDeleted);
			query.ExecuteUpdate();
		}

		public void UpdateShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				@"exec mart.[etl_dim_shift_category_update]
						@shift_category_code=:ShiftCategoryCode, 
						@shift_category_name=:ShiftCategoryName, 
						@shift_category_shortname=:ShiftCategoryShortname, 
						@display_color=:DisplayColor, 
						@business_unit_id=:BusinessUnitId,
						@datasource_id=:DatasourceId,
						@datasource_update_date=:DatasourceUpdateDate,
						@is_deleted=:IsDeleted
					  ")
				.SetGuid("ShiftCategoryCode", analyticsShiftCategory.ShiftCategoryCode)
				.SetString("ShiftCategoryName", analyticsShiftCategory.ShiftCategoryName)
				.SetString("ShiftCategoryShortname", analyticsShiftCategory.ShiftCategoryShortname)
				.SetInt32("DisplayColor", analyticsShiftCategory.DisplayColor)
				.SetInt32("BusinessUnitId", analyticsShiftCategory.BusinessUnitId)
				.SetInt32("DatasourceId", analyticsShiftCategory.DatasourceId)
				.SetDateTime("DatasourceUpdateDate", analyticsShiftCategory.DatasourceUpdateDate)
				.SetBoolean("IsDeleted", analyticsShiftCategory.IsDeleted);
			query.ExecuteUpdate();
		}
	}
}