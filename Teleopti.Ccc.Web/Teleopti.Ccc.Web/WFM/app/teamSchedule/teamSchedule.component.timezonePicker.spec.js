describe('timezonePicker component tests', function() {

	var $componentController;


	beforeEach(module('wfm.teamSchedule'));
	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	beforeEach(function () {
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$componentController_) {
		$componentController = _$componentController_;
	}));

	it('should expose default timezone to be currentUserInfo timezone', function() {
		var bindings = { availableTimezones: [], onPick: function () { } };
		var ctrl = $componentController('timezonePicker', null, bindings);
		ctrl.$onInit();
		expect(ctrl.selectedTimezone).toEqual("Asia/Hong_Kong");
	});

	it("should populate timezone list", inject(function () {
		var bindings = {
			 availableTimezones: [
			 {
			 	IanaId: "Asia/Shanghai",
			 	DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			 }, {
			 	IanaId: "Europe/Berlin",
			 	DisplayName: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			 }],
			 onPick: function () { }
		};
		var ctrl = $componentController('timezonePicker', null, bindings);
		ctrl.$onInit();
	
		expect(ctrl.timezoneList.length).toEqual(3);
		expect(ctrl.timezoneList[0].ianaId).toEqual("Asia/Hong_Kong");
		expect(ctrl.timezoneList[1].ianaId).toEqual("Asia/Shanghai");
		expect(ctrl.timezoneList[2].ianaId).toEqual("Europe/Berlin");
	}));

	it("should extract the right abbreviation of the selected time zone ", inject(function () {
		var bindings = {
			availableTimezones: [
			{
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				DisplayName: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			}],
			onPick: function () { }
		};
		var ctrl = $componentController('timezonePicker', null, bindings);
		ctrl.$onInit();

		ctrl.selectedTimezone = "Asia/Shanghai";
		var displayName = ctrl.shortDisplayNameOfTheSelected();
		expect(displayName).toEqual("UTC+08:00");		
	}));

	it("Should trigger onPick when selection changes", function () {
		var triggeredTimezone;
		var bindings = {
			availableTimezones: [
			{
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				DisplayName: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			}],
			onPick: function (input) {				
				triggeredTimezone = input.timezone;
			}
		};
		var ctrl = $componentController('timezonePicker', null, bindings);
		ctrl.$onInit();

		ctrl.selectedTimezone = "Asia/Shanghai";
		ctrl.onSelectionChanged();
		expect(triggeredTimezone).toEqual("Asia/Shanghai");
	});

});