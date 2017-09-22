using System;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class PossibleShiftTradePersonsProvider : IPossibleShiftTradePersonsProvider
	{
		private readonly ISettingsPersisterAndProvider<NameFormatSettings> _nameFormatSettings;
		private readonly IShiftTradePersonProvider _shiftTradePersonProvider;
		private readonly IToggleManager _toggleManager;

		public PossibleShiftTradePersonsProvider(ISettingsPersisterAndProvider<NameFormatSettings> nameFormatSettings, IShiftTradePersonProvider shiftTradePersonProvider, IToggleManager toggleManager)
		{
			_nameFormatSettings = nameFormatSettings;
			_shiftTradePersonProvider = shiftTradePersonProvider;
			_toggleManager = toggleManager;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments)
		{
		    var nameFormatSetting = _nameFormatSettings.Get().ToNameFormatSetting();

			var personList = _toggleManager.IsEnabled (Toggles.MyTimeWeb_ShiftTradeOptimization_40792)
				? _shiftTradePersonProvider.RetrievePeopleOptimized (shiftTradeArguments.ShiftTradeDate,
					shiftTradeArguments.TeamIdList.ToArray(), shiftTradeArguments.SearchNameText, nameFormatSetting)
				: _shiftTradePersonProvider.RetrievePersons (shiftTradeArguments.ShiftTradeDate,
					shiftTradeArguments.TeamIdList.ToArray(), shiftTradeArguments.SearchNameText, nameFormatSetting);


			var datePersons =  new DatePersons
				{
					Date = shiftTradeArguments.ShiftTradeDate,
					Persons = personList
			};

			return datePersons;
		}

		public DatePersons RetrievePersons(ShiftTradeScheduleViewModelData shiftTradeArguments, Guid[] personIds)
		{
			var personList = _shiftTradePersonProvider.RetrievePeopleOptimized(shiftTradeArguments.ShiftTradeDate, personIds);


			var datePersons = new DatePersons
			{
				Date = shiftTradeArguments.ShiftTradeDate,
				Persons = personList
			};

			return datePersons;

		}
	}
}