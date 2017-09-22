using System.Collections.Generic;
using NHibernate.Transform;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

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
				$@"select [shift_category_id] {nameof(AnalyticsShiftCategory.ShiftCategoryId)}
					,[shift_category_code] {nameof(AnalyticsShiftCategory.ShiftCategoryCode)}
					,[shift_category_name] {nameof(AnalyticsShiftCategory.ShiftCategoryName)}
					,[shift_category_shortname] {nameof(AnalyticsShiftCategory.ShiftCategoryShortname)}
					,[display_color] {nameof(AnalyticsShiftCategory.DisplayColor)}
					,[business_unit_id] {nameof(AnalyticsShiftCategory.BusinessUnitId)}
					,[datasource_id] {nameof(AnalyticsShiftCategory.DatasourceId)}
					,[insert_date] {nameof(AnalyticsShiftCategory.InsertDate)}
					,[update_date] {nameof(AnalyticsShiftCategory.UpdateDate)}
					,[datasource_update_date] {nameof(AnalyticsShiftCategory.DatasourceUpdateDate)}
					,[is_deleted] {nameof(AnalyticsShiftCategory.IsDeleted)}
					from [mart].[dim_shift_category] WITH (NOLOCK)")
				.SetResultTransformer(Transformers.AliasToBean(typeof (AnalyticsShiftCategory)))
				.SetReadOnly(true)
				.List<AnalyticsShiftCategory>();
		}

		public void AddShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_shift_category_insert]
						@shift_category_code=:{nameof(AnalyticsShiftCategory.ShiftCategoryCode)}, 
						@shift_category_name=:{nameof(AnalyticsShiftCategory.ShiftCategoryName)}, 
						@shift_category_shortname=:{nameof(AnalyticsShiftCategory.ShiftCategoryShortname)}, 
						@display_color=:{nameof(AnalyticsShiftCategory.DisplayColor)}, 
						@business_unit_id=:{nameof(AnalyticsShiftCategory.BusinessUnitId)},
						@datasource_id=:{nameof(AnalyticsShiftCategory.DatasourceId)},
						@datasource_update_date=:{nameof(AnalyticsShiftCategory.DatasourceUpdateDate)},
						@is_deleted=:{nameof(AnalyticsShiftCategory.IsDeleted)}
					  ")
				.SetGuid(nameof(AnalyticsShiftCategory.ShiftCategoryCode), analyticsShiftCategory.ShiftCategoryCode)
				.SetString(nameof(AnalyticsShiftCategory.ShiftCategoryName), analyticsShiftCategory.ShiftCategoryName)
				.SetString(nameof(AnalyticsShiftCategory.ShiftCategoryShortname), analyticsShiftCategory.ShiftCategoryShortname)
				.SetInt32(nameof(AnalyticsShiftCategory.DisplayColor), analyticsShiftCategory.DisplayColor)
				.SetInt32(nameof(AnalyticsShiftCategory.BusinessUnitId), analyticsShiftCategory.BusinessUnitId)
				.SetInt32(nameof(AnalyticsShiftCategory.DatasourceId), analyticsShiftCategory.DatasourceId)
				.SetDateTime(nameof(AnalyticsShiftCategory.DatasourceUpdateDate), analyticsShiftCategory.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsShiftCategory.IsDeleted), analyticsShiftCategory.IsDeleted);
			query.ExecuteUpdate();
		}

		public void UpdateShiftCategory(AnalyticsShiftCategory analyticsShiftCategory)
		{
			var query = _currentAnalyticsUnitOfWork.Current().Session().CreateSQLQuery(
				$@"exec mart.[etl_dim_shift_category_update]
						@shift_category_code=:{nameof(AnalyticsShiftCategory.ShiftCategoryCode)}, 
						@shift_category_name=:{nameof(AnalyticsShiftCategory.ShiftCategoryName)}, 
						@shift_category_shortname=:{nameof(AnalyticsShiftCategory.ShiftCategoryShortname)}, 
						@display_color=:{nameof(AnalyticsShiftCategory.DisplayColor)}, 
						@business_unit_id=:{nameof(AnalyticsShiftCategory.BusinessUnitId)},
						@datasource_id=:{nameof(AnalyticsShiftCategory.DatasourceId)},
						@datasource_update_date=:{nameof(AnalyticsShiftCategory.DatasourceUpdateDate)},
						@is_deleted=:{nameof(AnalyticsShiftCategory.IsDeleted)}
					  ")
				.SetGuid(nameof(AnalyticsShiftCategory.ShiftCategoryCode), analyticsShiftCategory.ShiftCategoryCode)
				.SetString(nameof(AnalyticsShiftCategory.ShiftCategoryName), analyticsShiftCategory.ShiftCategoryName)
				.SetString(nameof(AnalyticsShiftCategory.ShiftCategoryShortname), analyticsShiftCategory.ShiftCategoryShortname)
				.SetInt32(nameof(AnalyticsShiftCategory.DisplayColor), analyticsShiftCategory.DisplayColor)
				.SetInt32(nameof(AnalyticsShiftCategory.BusinessUnitId), analyticsShiftCategory.BusinessUnitId)
				.SetInt32(nameof(AnalyticsShiftCategory.DatasourceId), analyticsShiftCategory.DatasourceId)
				.SetDateTime(nameof(AnalyticsShiftCategory.DatasourceUpdateDate), analyticsShiftCategory.DatasourceUpdateDate)
				.SetBoolean(nameof(AnalyticsShiftCategory.IsDeleted), analyticsShiftCategory.IsDeleted);
			query.ExecuteUpdate();
		}
	}
}