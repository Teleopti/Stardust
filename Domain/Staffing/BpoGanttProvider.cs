using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class BpoGanttProvider
	{
		private readonly ISkillCombinationBpoTimeLineReader _skillCombinationBpoTimeLineReader;
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly INow _now;
		private readonly IUserTimeZone _userTimeZone;
		private readonly IUserUiCulture _userUiCulture;
		private readonly IUserCulture _userCulture;

		public BpoGanttProvider(ISkillCombinationBpoTimeLineReader skillCombinationBpoTimeLineReader, IStaffingSettingsReader staffingSettingsReader, 
			INow now, IUserTimeZone userTimeZone, IUserUiCulture userUiCulture, IUserCulture userCulture)
		{
			_skillCombinationBpoTimeLineReader = skillCombinationBpoTimeLineReader;
			_staffingSettingsReader = staffingSettingsReader;
			_now = now;
			_userTimeZone = userTimeZone;
			_userUiCulture = userUiCulture;
			_userCulture = userCulture;
		}

		public BpoGanttData GetAllGanttDataForBpoTimeline()
		{
			var bpoGanttData = getGanttData();
			bpoGanttData.GanttDataPerBpoList =
				TransformTimelineModelToGanttData(_skillCombinationBpoTimeLineReader.GetAllDataForBpoTimeline().ToList());
			
			return bpoGanttData;
		}
	
		public BpoGanttData GetGanttDataForBpoTimelineOnSkill(Guid skillId)
		{
			var bpoGanttData = getGanttData();
			bpoGanttData.GanttDataPerBpoList =
				TransformTimelineModelToGanttData(_skillCombinationBpoTimeLineReader.GetBpoTimelineDataForSkill(skillId).ToList());
		
			return bpoGanttData;
		}

		public BpoGanttData GetGanttDataForBpoTimelineOnSkillGroup(Guid skillGroupId)
		{
			var bpoGanttData = getGanttData();

			bpoGanttData.GanttDataPerBpoList =
				TransformTimelineModelToGanttData(_skillCombinationBpoTimeLineReader.GetBpoTimelineDataForSkillGroup(skillGroupId)
					.ToList());
			
			return bpoGanttData;
		}

		private BpoGanttData getGanttData()
		{
			var historicalDays = (_staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelHistoricalHours, 8 * 24) / 24) - 1;
			var todaysDate = _now.UtcDateTime().Date;

			var periodStartDate = todaysDate.AddDays(-historicalDays);

			return new BpoGanttData
			{
				FromDate = periodStartDate,
				ToDate = todaysDate.AddDays(_staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14))
			};

		}
		public List<GanttDataPerBpo> TransformTimelineModelToGanttData(List<SkillCombinationResourceBpoTimelineModel> timelineModel)
		{
			var list = new List<GanttDataPerBpo>();
			var importedString = Resources.ResourceManager.GetString(nameof(Resources.ImportBpoImportInformationTooltipImported),
				_userUiCulture.GetUiCulture());
			var atString = Resources.ResourceManager.GetString(nameof(Resources.ImportBpoImportInformationTooltipAt),
				_userUiCulture.GetUiCulture());
			var bpoGroups = timelineModel.GroupBy(m => m.Source);
			foreach (var bpoGroup in bpoGroups)
			{
				var bpoGanttData = new GanttDataPerBpo {Name = bpoGroup.Key};

				BpoGanttDataTask currentTask = null;
				foreach (var bpoLine in bpoGroup)
				{
					if (currentTask != null && bpoLine.OnDate == currentTask.To && 
						currentTask.Name == bpoLine.ImportFilename && 
						bpoLine.ImportedDateTime == currentTask.ImportedDateTime)
					{ 
						currentTask.To = bpoLine.OnDate.AddDays(1);
					}
					else
					{
						currentTask = new BpoGanttDataTask
						{
							From = bpoLine.OnDate,
							To = bpoLine.OnDate.AddDays(1),
							Name = bpoLine.ImportFilename,
							ImportedDateTime = bpoLine.ImportedDateTime,
							TooltipImportTime = TimeZoneHelper.ConvertFromUtc(bpoLine.ImportedDateTime, _userTimeZone.TimeZone()).ToString("f", _userCulture.GetCulture()),
							Firstname = bpoLine.Firstname,
							Lastname = bpoLine.Lastname,
							TooltipImportedString = importedString,
							TooltipAtString = atString
						};
						bpoGanttData.Tasks.Add(currentTask);
					}
				}

				list.Add(bpoGanttData);
			}
			return list;
		}

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> ImportInfoOnSkill(Guid skillId, DateTime dateTime)
		{
			var startDateTimeUtc = TimeZoneHelper.ConvertToUtc(dateTime.Date, _userTimeZone.TimeZone());
			return convertToUserTime(_skillCombinationBpoTimeLineReader.GetBpoImportInfoForSkill(skillId, startDateTimeUtc,
				startDateTimeUtc.AddDays(1)));
		}

		public IEnumerable<SkillCombinationResourceBpoImportInfoModel> ImportInfoOnSkillGroup(Guid skillGroupId, DateTime dateTime)
		{
			var startDateTimeUtc = TimeZoneHelper.ConvertToUtc(dateTime.Date, _userTimeZone.TimeZone());
			return convertToUserTime(_skillCombinationBpoTimeLineReader.GetBpoImportInfoForSkillGroup(skillGroupId, startDateTimeUtc,
				startDateTimeUtc.AddDays(1)));
		}

		private IEnumerable<SkillCombinationResourceBpoImportInfoModel> convertToUserTime(
			IEnumerable<SkillCombinationResourceBpoImportInfoModel> models)
		{
			foreach (var model in models)
			{
				model.ImportedDateTimeString = TimeZoneHelper.ConvertFromUtc(model.ImportedDateTime, _userTimeZone.TimeZone()).ToString("f", _userCulture.GetCulture());
			}

			return models;
		}
	}

	public class BpoGanttData
	{
		public DateTime FromDate{ get; set; }
		public DateTime ToDate{ get; set; }
		public List<GanttDataPerBpo> GanttDataPerBpoList = new List<GanttDataPerBpo>();
	}

	public class GanttDataPerBpo
	{
		[JsonProperty("height")]
		public string Height;
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("tasks")]
		public List<BpoGanttDataTask> Tasks = new List<BpoGanttDataTask>();
	}

	public class BpoGanttDataTask
	{		
		[JsonProperty("name")]
		public string Name;
		[JsonProperty("from")]
		public DateTime From;
		[JsonProperty("to")]
		public DateTime To;
		[JsonProperty("importeddatetime")]
		public DateTime ImportedDateTime;
		[JsonProperty("firstname")]
		public string Firstname;
		[JsonProperty("lastname")]
		public string Lastname;
		[JsonProperty("color")]
		public string Color = "#9FC5F8";
		[JsonProperty("tooltipimported")]
		public string TooltipImportedString;
		[JsonProperty("tooltipat")]
		public string TooltipAtString;
		[JsonProperty("tooltipimporttime")]
		public string TooltipImportTime;
	}

	public class SkillCombinationResourceBpoTimelineModel
	{
		public string Source { get; set; }
		public double Resources { get; set; }
		public DateTime OnDate { get; set; }
		public string ImportFilename { get; set; }
		public DateTime ImportedDateTime { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
	}

	public class SkillCombinationResourceBpoImportInfoModel
	{
		public string Source { get; set; }
		public string ImportFilename { get; set; }
		public DateTime ImportedDateTime { get; set; }
		public string ImportedDateTimeString { get; set; }
		public string Firstname { get; set; }
		public string Lastname { get; set; }
	}
}