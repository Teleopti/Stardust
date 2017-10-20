$(document).ready(function() {
	module('Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel');
	var fakeLayerData = {
		"Payload": "Phone",
		"ElapsedMinutesSinceShiftStart": 0,
		"Start": "/Date(-62135596800000)/",
		"End": "/Date(-62135596800000)/",
		"LengthInMinutes": 135,
		"Color": "#80FF80",
		"TitleHeader": null,
		"IsAbsenceConfidential": false,
		"TitleTime": "9:30 AM-11:45 AM"
	};

	test('should calculate layer position and width in percentage', function(){
		var minutesSinceTimeLineStart = 60, totalWidthInMinutes = 400;
		var vm = new Teleopti.MyTimeWeb.Request.LayerEditShiftTradeViewModel(fakeLayerData, minutesSinceTimeLineStart, totalWidthInMinutes);

		equal(vm.leftPercentage(), 100 * (0 + 60) / totalWidthInMinutes + '%');
		equal(vm.widthPercentage(), 100 * 135 / totalWidthInMinutes + '%');
	});
});