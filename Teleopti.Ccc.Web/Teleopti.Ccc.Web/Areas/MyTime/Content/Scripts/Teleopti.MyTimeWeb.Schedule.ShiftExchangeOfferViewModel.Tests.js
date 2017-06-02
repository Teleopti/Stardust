
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel");

	test("should post correct data for wish shift type", function() {
		var ajaxPostData;

		var ajax = {
			Ajax: function (options) {
				ajaxPostData = options.data;
			}
		};

		var option1 = { Id: 'working-shift', Description: 'Woking Shift' };
		var option2 = { Id: 'empty-day', Description: 'Empty Day' };

		var viewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, function () { });
		viewModel.WishShiftTypeOption(option2);
		viewModel.AllShiftTypes([option1, option2]);
		viewModel.DateTo(moment('2015-01-15'));
		viewModel.OfferValidTo(moment('2015-01-10'));
		viewModel.SaveShiftExchangeOffer();
		equal(ajaxPostData.WishShiftType, option2.Id);
	});


	test("when using persian display should validate time appropriately", function() {
		
		Teleopti.MyTimeWeb.Common.IsToggleEnabled = function (x) { return true; };
		Teleopti.MyTimeWeb.Common.SetupCalendar({
			UseJalaaliCalendar: true,
			DateFormat: 'YYYY/MM/DD',
			TimeFormat: 'HH:mm tt',
			AMDesignator: 'ق.ظ',
			PMDesignator: 'ب.ظ'
		});

		var shiftExchangeOfferViewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel();
		shiftExchangeOfferViewModel.StartTime("08:00 ق.ظ");
		shiftExchangeOfferViewModel.EndTime("05:00 ب.ظ");

		equal(shiftExchangeOfferViewModel.IsTimeLegal(), true);



	});

	test("should disable date and time selection input in mobile",
		function() {
			var fakeMobileWindow = {
				navigator: {
					appVersion: '5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1',
					userAgent: 'Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML, like Gecko) Version/9.0 Mobile/13B143 Safari/601.1'
				}
			}; 
			var vm = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(null, null, fakeMobileWindow);

			equal(vm.IsTimeInputEditable, false);
		});
});