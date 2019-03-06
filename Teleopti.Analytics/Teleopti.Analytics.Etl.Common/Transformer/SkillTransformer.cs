using System;
using System.Collections.Generic;
using System.Data;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;


namespace Teleopti.Analytics.Etl.Common.Transformer
{
	public class SkillTransformer
	{
		private readonly DateTime _insertDateTime;

		private SkillTransformer() { }

		public SkillTransformer(DateTime insertDateTime)
			: this()
		{
			_insertDateTime = insertDateTime;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public void Transform(IList<ISkill> rootList, DataTable skillTable)
		{
			InParameter.NotNull("rootList", rootList);
			InParameter.NotNull("skillTable", skillTable);

			foreach (ISkill skill in rootList)
			{
				DataRow dataRow = skillTable.NewRow();

				dataRow["skill_code"] = skill.Id;
				dataRow["skill_name"] = skill.Name;
				dataRow["time_zone_code"] = skill.TimeZone.Id;
				dataRow["forecast_method_code"] = skill.SkillType.Id;
				dataRow["forecast_method_name"] = skill.SkillType.ForecastSource.ToString();
				dataRow["business_unit_code"] = skill.GetOrFillWithBusinessUnit_DONTUSE().Id;
				dataRow["business_unit_name"] = skill.GetOrFillWithBusinessUnit_DONTUSE().Name;
				dataRow["datasource_id"] = 1; //The Matrix internal id. Raptor = 1.
				dataRow["insert_date"] = _insertDateTime;
				dataRow["update_date"] = _insertDateTime;
				dataRow["datasource_update_date"] = RaptorTransformerHelper.GetUpdatedDate(skill);

				IDeleteTag deleteCheck = skill as IDeleteTag;
				if (deleteCheck != null)
				{
					dataRow["is_deleted"] = deleteCheck.IsDeleted;
				}

				skillTable.Rows.Add(dataRow);
			}
		}
	}
}
