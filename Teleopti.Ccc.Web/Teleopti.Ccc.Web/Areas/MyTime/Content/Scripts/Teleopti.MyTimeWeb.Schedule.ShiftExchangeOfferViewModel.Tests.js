$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel');

	var resetLocale = function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			DateFormat: 'YYYY-MM-DD',
			UseJalaaliCalendar: false
		});
	};

	test('should post correct data for wish shift type', function() {
		var ajaxPostData;

		var ajax = {
			Ajax: function(options) {
				ajaxPostData = options.data;
			}
		};

		var option1 = { Id: 'working-shift', Description: 'Woking Shift' };
		var option2 = { Id: 'empty-day', Description: 'Empty Day' };

		var viewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, function() {});
		viewModel.WishShiftTypeOption(option2);
		viewModel.AllShiftTypes([option1, option2]);
		viewModel.DateTo(moment('2015-01-15'));
		viewModel.OfferValidTo(moment('2015-01-10'));
		viewModel.SaveShiftExchangeOffer();
		equal(ajaxPostData.WishShiftType, option2.Id);
	});

	test('when using persian display should validate time appropriately', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel();
		shiftExchangeOfferViewModel.StartTime('08:00 ق.ظ');
		shiftExchangeOfferViewModel.EndTime('05:00 ب.ظ');

		equal(shiftExchangeOfferViewModel.IsTimeLegal(), true);

		resetLocale();
	});

	test('should set correct time format for fi-fi', function() {
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: false,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'H.mm'
		});

		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel();
		shiftExchangeOfferViewModel.StartTime('08:00');
		shiftExchangeOfferViewModel.EndTime('17:00');

		equal(shiftExchangeOfferViewModel.startTimeInternal(), '08.00');
		equal(shiftExchangeOfferViewModel.endTimeInternal(), '17.00');
	});

	test('should call getAbsence service for only one time when loading Post Shift for Trade page', function () {
		var counter = 0;
		var ajax = {
			Ajax: function () {
				counter++;
			}
		};
		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, function () { });
		shiftExchangeOfferViewModel.DateToForPublish('2018/11/14');
		shiftExchangeOfferViewModel.DateToForPublish('2018/11/14');
		equal(counter, 1);
	});
});
