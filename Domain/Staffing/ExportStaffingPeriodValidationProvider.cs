using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ExportStaffingPeriodValidationProvider
	{
		private readonly IStaffingSettingsReader _staffingSettingsReader;
		private readonly INow _now;
		private readonly IUserCulture _userCulture;
		private readonly IUserUiCulture _userUiCulture;

		public ExportStaffingPeriodValidationProvider(IStaffingSettingsReader staffingSettingsReader, INow now, IUserCulture userCulture,
			IUserUiCulture userUiCulture)
		{
			_staffingSettingsReader = staffingSettingsReader;
			_now = now;
			_userCulture = userCulture;
			_userUiCulture = userUiCulture;
		}

		public ExportStaffingValidationObject ValidateExportBpoPeriod(DateOnly periodStartDate, DateOnly periodEndDate)
		{
			var validationObject = new ExportStaffingValidationObject {Result = false};
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);
			if (periodStartDate > periodEndDate)
			{
				validationObject.Result = false;
				validationObject.ErrorMessage = Resources.ResourceManager.GetString(nameof(Resources.BpoExportPeriodStartDateBeforeEndDate), _userUiCulture.GetUiCulture());
				return validationObject;
			}
			if (periodStartDate < utcNowDate || periodEndDate > exportPeriodMaxDate)
			{
				validationObject.Result = false;
				validationObject.ErrorMessage =  GetExportGapPeriodMessageString();
				return validationObject;
			}

			validationObject.Result = true;
			return validationObject;
		}
		
		public ExportStaffingValidationObject ValidateExportStaffingPeriod(DateOnly periodStartDate, DateOnly periodEndDate)
		{
			var validationObject = new ExportStaffingValidationObject {Result = false};
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);
			var staffingReadModelHistoricalDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelHistoricalHours, 8 * 24)/24;
			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMinDate = utcNowDate.AddDays(-staffingReadModelHistoricalDays);
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);
			if (periodStartDate > periodEndDate)
			{
				validationObject.Result = false;
				validationObject.ErrorMessage = Resources.ResourceManager.GetString(nameof(Resources.BpoExportPeriodStartDateBeforeEndDate), _userUiCulture.GetUiCulture());
				return validationObject;
			}
			if (periodStartDate < exportPeriodMinDate || periodEndDate > exportPeriodMaxDate)
			{
				validationObject.Result = false;
				var validExportPeriodText = GetExportStaffingPeriodMessageString();
				validationObject.ErrorMessage = validExportPeriodText;
				return validationObject;
			}

			validationObject.Result = true;
			return validationObject;
		}
		
		public string GetExportStaffingPeriodMessageString()
		{
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);
			var staffingReadModelHistoricalDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelHistoricalHours, 8 * 24)/24;

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportMinDate = new DateOnly(_now.UtcDateTime().AddDays(-staffingReadModelHistoricalDays));
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);

			var validExportPeriodText =
				$"{exportMinDate.ToShortDateString(_userCulture.GetCulture())} - {exportPeriodMaxDate.ToShortDateString(_userCulture.GetCulture())}";
			var exportPeriodMessage = string.Format(
				Resources.ResourceManager.GetString(nameof(Resources.BpoOnlyExportPeriodBetweenDates), _userUiCulture.GetUiCulture()), validExportPeriodText);
			return exportPeriodMessage;
		}
		
		public string GetExportGapPeriodMessageString()
		{
			var readModelNumberOfDays = _staffingSettingsReader.GetIntSetting(KeyNames.StaffingReadModelNumberOfDays, 14);

			var utcNowDate = new DateOnly(_now.UtcDateTime());
			var exportMinDate = new DateOnly(_now.UtcDateTime());
			var exportPeriodMaxDate = utcNowDate.AddDays(readModelNumberOfDays);

			var validExportPeriodText =
				$"{exportMinDate.ToShortDateString(_userCulture.GetCulture())} - {exportPeriodMaxDate.ToShortDateString(_userCulture.GetCulture())}";
			var exportPeriodMessage = string.Format(
				Resources.ResourceManager.GetString(nameof(Resources.BpoOnlyExportPeriodBetweenDates), _userUiCulture.GetUiCulture()), validExportPeriodText);
			return exportPeriodMessage;
		}

	}
	
	public class ExportStaffingValidationObject
	{
	 	public bool Result;
		public string ErrorMessage;
	}
}