
$(document).ready(function() {

	module("Teleopti.MyTimeWeb.Preference day view model");

	test("should load preference", function() {

		var ajax = {
			Ajax: function(options) {
				equal(options.url, "Preference/Preference");
				equal(options.data.Date, "2012-06-11");
				options.success({
					Preference: "a shift category",
					Color: "black",
					Extended: true,
					ExtendedTitle: "ExtendedTitle",
					StartTimeLimitation: "8:00 am-9:00 am",
					EndTimeLimitation: "5:00 pm-6:00 pm",
					WorkTimeLimitation: "8:00-9:00",
					Activity: "Lunch",
					ActivityStartTimeLimitation: "8:00 am-9:00 am",
					ActivityEndTimeLimitation: "5:00 pm-6:00 pm",
					ActivityTimeLimitation: "1:00-2:00"
				});
			}
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = "2012-06-11";

		expect(13);

		viewModelDay.LoadPreference();

		equal(viewModelDay.Preference(), "a shift category");
		equal(viewModelDay.Color(), "black");
		equal(viewModelDay.Extended(), true);
		equal(viewModelDay.ExtendedTitle(), "ExtendedTitle");
		equal(viewModelDay.StartTimeLimitation(), "8:00 am-9:00 am");
		equal(viewModelDay.EndTimeLimitation(), "5:00 pm-6:00 pm");
		equal(viewModelDay.WorkTimeLimitation(), "8:00-9:00");
		equal(viewModelDay.Activity(), "Lunch");
		equal(viewModelDay.ActivityStartTimeLimitation(), "8:00 am-9:00 am");
		equal(viewModelDay.ActivityEndTimeLimitation(), "5:00 pm-6:00 pm");
		equal(viewModelDay.ActivityTimeLimitation(), "1:00-2:00");
	});

	test("should bind extended", function() {

		var element = $("#qunit-fixture")
			.append("<div class='extended-indication' data-bind='visible: Extended' style='display: none'></div>")[0];

		var dayViewModel = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		ko.applyBindings(dayViewModel, element);

		dayViewModel.Extended(true);

		equal($(element).is(':visible'), true);
	});

	test("should set preference", function() {

		var ajax = {
			Ajax: function(options) {
				equal(options.url, "Preference/Preference");
				equal(options.data.Date, "2012-06-11");
				equal(options.data.PreferenceId, "id");
				options.success({
					Preference: "a shift category",
					Color: "black",
					Extended: false
				});
			}
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = "2012-06-11";

		expect(5);

		viewModelDay.SetPreference("id");

		equal(viewModelDay.Preference(), "a shift category");
		equal(viewModelDay.Color(), "black");
	});

	test("should set extended preference", function() {

		var ajax = {
			Ajax: function(options) {
				equal(options.url, "Preference/Preference");
				equal(options.data.Date, "2012-09-03", "Date");
				equal(options.data.PreferenceId, "id1", "PreferenceId");
				equal(options.data.StartTimeMinimum, "8:00", "StartTimeMinimum");
				equal(options.data.StartTimeMaximum, "9:00", "StartTimeMaximum");
				equal(options.data.EndTimeMinimum, "15:00");
				equal(options.data.EndTimeMaximum, "18:00");
				equal(options.data.WorkTimeMinimum, "6:00");
				equal(options.data.WorkTimeMaximum, "8:00");
				equal(options.data.ActivityId, "id2");
				equal(options.data.ActivityStartTimeMinimum, "11:00");
				equal(options.data.ActivityStartTimeMaximum, "12:00");
				equal(options.data.ActivityEndTimeMinimum, "12:02");
				equal(options.data.ActivityEndTimeMaximum, "13:00");
				equal(options.data.ActivityTimeMinimum, "1:00");
				equal(options.data.ActivityTimeMaximum, "0:30");
				options.success({
					Preference: "a shift category",
					Color: "black",
					Extended: true
				});
			}
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = "2012-09-03";

		expect(16 + 1);

		viewModelDay.SetPreference({
			PreferenceId: "id1",
			StartTimeMinimum: "8:00",
			StartTimeMaximum: "9:00",
			EndTimeMinimum: "15:00",
			EndTimeMaximum: "18:00",
			WorkTimeMinimum: "6:00",
			WorkTimeMaximum: "8:00",
			ActivityId: "id2",
			ActivityStartTimeMinimum: "11:00",
			ActivityStartTimeMaximum: "12:00",
			ActivityEndTimeMinimum: "12:02",
			ActivityEndTimeMaximum: "13:00",
			ActivityTimeMinimum: "1:00",
			ActivityTimeMaximum: "0:30"
		});

		equal(viewModelDay.Extended(), true);
	});

	test("should delete preference", function() {

		var ajax = {
			Ajax: function(options) {
				equal(options.url, "Preference/Preference");
				equal(options.data.Date, "2012-06-11");
				options.success({
					Preference: "deleted!",
					Color: "deleted!",
					Extended: "deleted!"
				});
			}
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = "2012-06-11";

		expect(5);

		viewModelDay.DeletePreference();

		equal(viewModelDay.Preference(), "deleted!");
		equal(viewModelDay.Color(), "deleted!");
		equal(viewModelDay.Extended(), "deleted!");
	});

	test("should format possible contract time", function() {
		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();
		viewModelDay.PossibleContractTimeMinutesLower(6 * 60 + 30);
		viewModelDay.PossibleContractTimeMinutesUpper(8 * 60 + 5);

		expect(2);
		equal(viewModelDay.PossibleContractTimeLower(), "6:30");
		equal(viewModelDay.PossibleContractTimeUpper(), "8:05");
	});

	test("should load feedback", function() {

		var ajax = {
			Ajax: function(options) {
				options.success({
					FeedbackError: "an error",
					PossibleStartTimes: "6:00-9:00",
					PossibleEndTimes: "16:00-18:00",
					PossibleContractTimeMinutesLower: 7 * 60,
					PossibleContractTimeMinutesUpper: 12 * 60
				});
			}
		};

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel(ajax);
		viewModelDay.Date = "2012-06-11";

		viewModelDay.LoadFeedback();

		equal(viewModelDay.FeedbackError(), "an error");
		equal(viewModelDay.PossibleStartTimes(), "6:00-9:00");
		equal(viewModelDay.PossibleEndTimes(), "16:00-18:00");
		equal(viewModelDay.PossibleContractTimes(), "7:00-12:00");
	});

	test("should compute DisplayFeedbackError", function() {

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		equal(viewModelDay.DisplayFeedbackError(), false, "compute not initialized");

		viewModelDay.FeedbackError(null);
		equal(viewModelDay.DisplayFeedbackError(), false, "compute with null");

		viewModelDay.FeedbackError(undefined);
		equal(viewModelDay.DisplayFeedbackError(), false, "compute with undefined");

		viewModelDay.FeedbackError("");
		equal(viewModelDay.DisplayFeedbackError(), false, "compute with empty string");

		viewModelDay.FeedbackError("an error");
		equal(viewModelDay.DisplayFeedbackError(), true, "compute with string");

	});

	test("should compute DisplayFeedback", function() {

		var viewModelDay = new Teleopti.MyTimeWeb.Preference.DayViewModel();

		equal(viewModelDay.DisplayFeedback(), false, "compute not initialized");

		var resetViewModel = function() {
			viewModelDay.PossibleStartTimes(undefined);
			viewModelDay.PossibleEndTimes(undefined);
			viewModelDay.PossibleContractTimeMinutesLower(undefined);
			viewModelDay.PossibleContractTimeMinutesUpper(undefined);
			viewModelDay.FeedbackError(undefined);
		};

		resetViewModel();
		viewModelDay.PossibleStartTimes("a value");
		viewModelDay.FeedbackError("an error");
		equal(viewModelDay.DisplayFeedback(), false, "compute with FeedbackError");

		resetViewModel();
		viewModelDay.PossibleStartTimes("a value");
		equal(viewModelDay.DisplayFeedback(), true, "compute with PossibleStartTimes");

		resetViewModel();
		viewModelDay.PossibleEndTimes("a value");
		equal(viewModelDay.DisplayFeedback(), true, "compute with PossibleEndTimes");

		resetViewModel();
		viewModelDay.PossibleContractTimeMinutesLower("a value");
		equal(viewModelDay.DisplayFeedback(), true, "compute with PossibleContractTimeMinutesLower");

		resetViewModel();
		viewModelDay.PossibleContractTimeMinutesUpper("a value");
		equal(viewModelDay.DisplayFeedback(), true, "compute with PossibleContractTimeMinutesUpper");

	});

});