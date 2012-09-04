﻿
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference initializer");

	test("should load preferences", function () {

		$("#qunit-fixture")
			.append("<li data-mytime-date='2012-06-11' class='inperiod preference' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod preference' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "Preference/Preference")
					return;
				if (options.data.Date == '2012-06-11')
					equal(true, true);
				if (options.data.Date == '2012-06-12')
					equal(true, true);
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();
	});

	test("should only load preference for days with class preference", function () {

		$("#qunit-fixture")
			.append("<li data-mytime-date='2012-06-11' class='inperiod' ><div><div class='preference'></div></div><li>")
			.append("<li data-mytime-date='2012-06-12' class='inperiod' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "Preference/Preference")
					return;
				if (options.data.Date == '2012-06-11')
					return equal(true, true, "preference loaded");
				equal(false, true, "preference loaded for " + options.data.Date);
			}
		};

		expect(1);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

	});


	test("should load day feedback and bind", function () {

		$("#qunit-fixture")
			.html("<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>")
			.append("<li data-mytime-date='2012-06-11' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "PreferenceFeedback/Feedback")
					return;
				if (options.data.Date == '2012-06-11')
					options.success({
						PossibleContractTimeMinutesLower: 6 * 60
					});
				if (options.data.Date == '2012-06-12')
					options.success({
						PossibleContractTimeMinutesLower: 8 * 60
					});
			}
		};

		expect(3);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('#Preference-period-feedback-view').text(), "14:00");
		equal($('li[data-mytime-date="2012-06-11"]').text(), "6:00");
		equal($('li[data-mytime-date="2012-06-12"]').text(), "8:00");
	});

	test("should only load feedback for days with class feedback", function () {
		$("#qunit-fixture")
			.html("<li data-mytime-date='2012-06-11' data-bind='text: PossibleContractTimeLower' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "PreferenceFeedback/Feedback")
					return;
				options.success({
					PossibleContractTimeMinutesLower: 8 * 60
				});
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('li[data-mytime-date="2012-06-11"]').text(), "");
		equal($('li[data-mytime-date="2012-06-12"]').text(), "8:00");
	});

	test("should compute with static schedule data", function () {

		$("#qunit-fixture")
			.html("<div id='Preference-period-feedback-view'><span data-bind='text: PossibleResultContractTimeLower' /><span data-bind='text: PossibleResultContractTimeUpper' /></div>")
			.append("<li data-mytime-date='2012-06-13' class='inperiod'><span data-mytime-contract-time='120' /></li>")
			.append("<li data-mytime-date='2012-06-14' class='inperiod'><span data-mytime-contract-time='60' /></li>");

		var ajax = {
			Ajax: function (options) {
				if (options.url == "PreferenceFeedback/Feedback")
					ok(true, "feedback should not be loaded");
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeLower"]').text(), "3:00", "lower contract time");
		equal($('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeUpper"]').text(), "3:00", "upper contract time");
	});

	test("should load period feedback", function () {

		var ajax = {
			Ajax: function (options) {
				equal(options.url, "PreferenceFeedback/PeriodFeedback", "period feedback ajax url");
				equal(options.data.Date, "2012-06-13", "period feedback date");
			}
		};
		var portal = {
			CurrentFixedDate: function () { return "2012-06-13"; }
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax, portal);

		target.InitViewModels();

	});

	test("should clear day view models on init", function () {

		$("#qunit-fixture")
			.html("<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>")
			.append("<li data-mytime-date='2012-06-19' class='inperiod feedback' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "PreferenceFeedback/Feedback")
					return;
				if (options.data.Date == '2012-06-19' || options.data.Date == '2012-06-20')
					options.success({
						PossibleContractTimeMinutesLower: 6 * 60
					});
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		$("#qunit-fixture")
			.html("<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>")
			.append("<li data-mytime-date='2012-06-20' class='inperiod feedback' />");

		target.InitViewModels();

		equal($('#Preference-period-feedback-view').text(), "6:00", "day sum after reinit same instance");
	});
	
});
