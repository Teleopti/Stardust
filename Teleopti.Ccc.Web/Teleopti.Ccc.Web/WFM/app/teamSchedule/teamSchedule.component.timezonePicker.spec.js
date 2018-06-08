describe('<timezone-picker>', function () {
	var $componentController,
		$rootScope,
		$compile,
		TimezoneListFactory,
		timezoneDataService;

	var mockCurrentUserInfo = {
		CurrentUserInfo: function () {
			return { DefaultTimeZone: "Asia/Hong_Kong", DefaultTimeZoneName: '(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi' };
		}
	};

	beforeEach(function () {
		module('wfm.templates');
		module("wfm.teamSchedule");
		module(function ($provide) {
			
			$provide.service('CurrentUserInfo', function () {
				return mockCurrentUserInfo;
			});
			$provide.service('TimezoneDataService', function () {
				timezoneDataService = new TimezoneDataService();
				return timezoneDataService;
			});
		});
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_, _TimezoneListFactory_) {
		$componentController = _$componentController_;
		$rootScope = _$rootScope_;
		$compile = _$compile_;
		TimezoneListFactory = _TimezoneListFactory_;
	}));

	it("should list timezone order by display name", function () {
		var panel = setUp();
		var timezoneSelect = panel[0].querySelectorAll("md-select");
		var timezoneOptions = panel[0].querySelectorAll("md-option");
		var selectionDisplayValue = panel[0].querySelector("md-select-value");

		expect(timezoneSelect.length).toEqual(1);
		expect(timezoneOptions.length).toEqual(2);
		expect(timezoneOptions[0].innerText.trim()).toEqual("(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna");
		expect(timezoneOptions[1].innerText.trim()).toEqual("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi");
		expect(selectionDisplayValue.innerText).toEqual('UTC+08:00');
	});

	it('should include current user timezone if avaliable timezones without it', function () {
		timezoneDataService.setAll([
			{
				IanaId: "Asia/Hong_Kong",
				Name: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				Name: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			},
			{
				IanaId: "America/New_York",
				Name: "(UTC-05:00) Estern Time"
			}
		]);
		var panel = setUp(null, ["America/New_York"]);
		var timezoneOptions = panel[0].querySelectorAll("md-option");
		expect(timezoneOptions[0].innerText.trim()).toEqual("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi");
		expect(timezoneOptions[1].innerText.trim()).toEqual("(UTC-05:00) Estern Time");
	});

	it('should expose default timezone to be currentUserInfo timezone', function () {
		var panel = setUp();
		var selectedOption = panel[0].querySelector('md-option[selected] .md-text');
		var selectedValueEl = panel[0].querySelector('md-select-value');
		expect(selectedOption.innerText).toEqual('(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi');
		expect(selectedValueEl.innerText).toEqual('UTC+08:00');
	});

	it('should pre selected the timezone', function () {
		var panel = setUp('Europe/Berlin');
		var selectedOption = panel[0].querySelector('md-option[selected] .md-text');
		expect(selectedOption.innerText).toEqual('(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna');
	});


	it("should update ngModel value when selection changed", function () {
		var panel = setUp();
		var timezoneOptions = panel[0].querySelectorAll("md-option");
		timezoneOptions[0].click();

		var scope = panel.isolateScope().$parent;
		expect(scope.timezone).toEqual('Europe/Berlin');
	});

	it('should set selectedTimezone to current user timezone if avaliable timezones does not contain the selected timezone', function () {

		var panel = setUp('America/New_York');
		var selectedOption = panel[0].querySelector('md-option[selected] .md-text');
		expect(selectedOption.innerText).toEqual('(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi');
	});

	function TimezoneDataService() {
		var timezones = [
			{
				IanaId: "Asia/Hong_Kong",
				Name: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				Name: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			}
		];

		this.setAll = function (tzs) {
			timezones = tzs;
		}
		this.getAll = function () {
			return {
				then: function (callback) {
					callback({
						Timezones: timezones
					});
				}
			};
		}
	}


	function setUp(timezone, availableTimezones) {
		var scope = $rootScope.$new();

		var html = '<timezone-picker class="timezone-picker" ng-model="timezone" avaliable-timezones="availableTimezones"></timezone-picker> ';
		var availableTimezones = availableTimezones || ["Asia/Hong_Kong", "Europe/Berlin"];
		scope.availableTimezones = availableTimezones;
		scope.timezone = timezone;

		var element = $compile(html)(scope);
		scope.$apply();

		return element;

	}

});