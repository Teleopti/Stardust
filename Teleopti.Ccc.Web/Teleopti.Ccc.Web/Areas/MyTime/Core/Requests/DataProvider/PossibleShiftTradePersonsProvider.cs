using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;
		private readonly IShiftTradePersonProvider _shiftTradePersonProvider;

		public PossibleShiftTradePersonsProvider(ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings, IShiftTradePersonProvider shiftTradePersonProvider)
		{
			_nameFormatSettings = nameFormatSettings;
			_shiftTradePersonProvider = shiftTradePersonProvider;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
		    var nameFormatSetting = _nameFormatSettings.Get().ToNameFormatSetting();

			var datePersons =  new DatePersons
				{
					Date = shiftTradeArguments.ShiftTradeDate,
					Persons =
						_shiftTradePersonProvider.RetrievePersons (shiftTradeArguments.ShiftTradeDate, shiftTradeArguments.TeamIdList.ToArray(), shiftTradeArguments.SearchNameText, nameFormatSetting)
			};

			return datePersons;
		}
	}
}