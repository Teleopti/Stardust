describe('timezonePicker component tests', function() {

	var $componentController,
		$rootScope,
		$compile;


	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong" };
		}
	};

	beforeEach(function () {
		module('wfm.templates');
		module("wfm.teamSchedule");
		module(function ($provide) {
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
		});
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_) {
		$componentController = _$componentController_;
		$rootScope = _$rootScope_;
		$compile = _$compile_;
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

	it("should list timezone order by displayName", function () {
		var panel = setUp();
		var timezoneSelect = panel[0].querySelectorAll("md-select");
		var timezoneOptions = panel[0].querySelectorAll("md-option");

		expect(timezoneSelect.length).toEqual(1);
		expect(timezoneOptions.length).toEqual(3);
		expect(timezoneOptions[0].innerText.trim()).toEqual("(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna");
		expect(timezoneOptions[1].innerText.trim()).toEqual("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi");
		expect(timezoneOptions[2].innerText.trim()).toEqual("");
		
	});

	function setUp() {
		var scope = $rootScope.$new();
		var html = '<timezone-picker class="timezone-picker" on-pick="onPick" available-timezones="availableTimezones"></timezone-picker> ';
		scope.availableTimezones = [
			{
				IanaId: "Asia/Shanghai",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				DisplayName: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			}];
		scope.onPick = {};

		var element = $compile(html)(scope);
		scope.$apply();

		return element;

	}

});