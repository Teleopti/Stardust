
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
		viewModel.Toggle31317Enabled(true);
		viewModel.WishShiftTypeOption(option2);
		viewModel.AllShiftTypes([option1, option2]);
		viewModel.DateTo(moment('2015-01-15'));
		viewModel.OfferValidTo(moment('2015-01-10'));
		viewModel.SaveShiftExchangeOffer();
		equal(ajaxPostData.WishShiftType, option2.Id);
	});
});