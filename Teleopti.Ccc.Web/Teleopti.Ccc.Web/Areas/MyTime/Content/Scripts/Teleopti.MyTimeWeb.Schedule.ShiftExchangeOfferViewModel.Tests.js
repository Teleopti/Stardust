
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel");

	test("should post correct data for wish shift type", function() {
		var ajaxPostData;

		var ajax = {
			Ajax: function(options) {
				ajaxPostData = options.data;
			}
		};
		
		var viewModel = new Teleopti.MyTimeWeb.Schedule.ShiftExchangeOfferViewModel(ajax, function() {});
		viewModel.WishShiftType({ Id: 'working-shift', Description: 'Empty Day' });
		viewModel.DateTo(moment('2015-01-15'));
		viewModel.OfferValidTo(moment('2015-01-10'));
		viewModel.SaveShiftExchangeOffer();
		equal(ajaxPostData.WishShiftType, 'working-shift');
	});
});