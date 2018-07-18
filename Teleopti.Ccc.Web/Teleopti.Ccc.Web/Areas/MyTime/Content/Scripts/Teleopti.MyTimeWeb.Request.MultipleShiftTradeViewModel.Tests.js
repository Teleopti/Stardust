$(document).ready(function () {
	module("Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel");

	test("should show add button", function () {
		var viewModel = new Teleopti.MyTimeWeb.Request.MultipleShiftTradeViewModel();
		viewModel.agentChoosed({});
		viewModel.selectedInternal(false);
		viewModel.IsLoading(false);

		equal(viewModel.isAddVisible(), true);
	});
});