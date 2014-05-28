
$(document).ready(function () {

	module("Teleopti.MyTimeWeb.Preference initializer");

	test("should load preferences", function () {

		$("#qunit-fixture")
			.append("<li data-mytime-week='week' class='inperiod preference' />")
			.append("<li data-mytime-date='2012-06-11' class='inperiod preference' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod preference' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url != "Preference/PreferencesAndSchedules")
					return;
				equal(options.data.From, '2012-06-11');
				equal(options.data.To, '2012-06-12');
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();
	});

	test("should load day feedback and bind", function () {

		$("#qunit-fixture")
			.html("<div id='Preference-period-feedback-view' data-bind='text: PossibleResultContractTimeLower'>No data!</div>")
			.append("<li data-mytime-week='week' />")
			.append("<li data-mytime-date='2012-06-11' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url == "Preference/PreferencesAndSchedules") {
					options.success(
						[
							{ Date: "2012-06-11", Feedback: true },
							{ Date: "2012-06-12", Feedback: true }
						]
					);
				}
				if (options.url == "PreferenceFeedback/Feedback") {
					if (options.data.Date == '2012-06-11')
						options.success({
							PossibleContractTimeMinutesLower: 6 * 60
						});
					if (options.data.Date == '2012-06-12')
						options.success({
							PossibleContractTimeMinutesLower: 8 * 60
						});
				}
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
			.html("<li data-mytime-week='week' />")
			.append("<li data-mytime-date='2012-06-11' data-bind='text: PossibleContractTimeLower' />")
			.append("<li data-mytime-date='2012-06-12' class='inperiod feedback' data-bind='text: PossibleContractTimeLower' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url == "Preference/PreferencesAndSchedules") {
					options.success(
						[
							{ Date: "2012-06-12", Feedback: true }
						]
					);
				}
				if (options.url == "PreferenceFeedback/Feedback") {
					options.success({
						PossibleContractTimeMinutesLower: 8 * 60
					});
				}
			}
		};

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('li[data-mytime-date="2012-06-11"]').text(), "");
		equal($('li[data-mytime-date="2012-06-12"]').text(), "8:00");
	});

	test("should compute with schedule data", function () {

		$("#qunit-fixture")
			.append("<div id='Preference-period-feedback-view'><span data-bind='text: PossibleResultContractTimeLower' /><span data-bind='text: PossibleResultContractTimeUpper' /></div>")
			.append("<li data-mytime-week='week'></li>")
			.append("<li data-mytime-date='2012-06-13' class='inperiod'></li>")
			.append("<li data-mytime-date='2012-06-14' class='inperiod'></li>");

		var ajax = {
			Ajax: function (options) {
				if (options.url == "PreferenceFeedback/Feedback")
					ok(true, "feedback should not be loaded");
				if (options.url == "Preference/PreferencesAndSchedules") {
					var data1 = {
						Date: '2012-06-13',
						PersonAssignment: {
							ContractTimeMinutes: 120
						}
					};
					var data2 = {
						Date: '2012-06-14',
						PersonAssignment: {
							ContractTimeMinutes: 120
						}
					};
					var data = [data1, data2];
					options.success(data);
				}
			}
		};

		expect(2);

		var target = new Teleopti.MyTimeWeb.PreferenceInitializer(ajax);

		target.InitViewModels();

		equal($('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeLower"]').text(), "4:00", "lower contract time");
		equal($('#Preference-period-feedback-view [data-bind*="PossibleResultContractTimeUpper"]').text(), "4:00", "upper contract time");
	});

	test("should load period feedback", function () {

		var ajax = {
			Ajax: function (options) {
				if (options.url == "Preference/PreferencesAndSchedules") {
					options.success();
				}
				if (options.url == "PreferenceFeedback/PeriodFeedback") {
					equal(options.url, "PreferenceFeedback/PeriodFeedback", "period feedback ajax url");
					equal(options.data.Date, "2012-06-13", "period feedback date");
				}
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
			.append("<li data-mytime-week='week' />")
			.append("<li data-mytime-date='2012-06-19' class='inperiod feedback' />");

		var ajax = {
			Ajax: function (options) {
				if (options.url == "Preference/PreferencesAndSchedules") {
					if (options.data.From == '2012-06-19') {
						options.success(
							[
								{ Date: "2012-06-19", Feedback: true }
							]
						);
					}
					if (options.data.From == '2012-06-20') {
						options.success(
							[
								{ Date: "2012-06-20", Feedback: true }
							]
						);
					}
				}
				if (options.url == "PreferenceFeedback/Feedback") {
					if (options.data.Date == '2012-06-19' || options.data.Date == '2012-06-20')
						options.success({
							PossibleContractTimeMinutesLower: 6 * 60
						});
				}
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
