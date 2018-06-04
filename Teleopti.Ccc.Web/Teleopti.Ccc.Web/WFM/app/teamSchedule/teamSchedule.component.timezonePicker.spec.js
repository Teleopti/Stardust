describe('<timezonePicker component tests>', function () {
	var $componentController,
		$rootScope,
		$compile;

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
		});
	});

	beforeEach(inject(function (_$componentController_, _$rootScope_, _$compile_) {
		$componentController = _$componentController_;
		$rootScope = _$rootScope_;
		$compile = _$compile_;
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
		var panel = setUp(null, [
			{
				IanaId: "America/New_York",
				DisplayName: "(UTC-05:00) Estern Time"
			}
		]);
		var timezoneOptions = panel[0].querySelectorAll("md-option");
		expect(timezoneOptions[0].innerText.trim()).toEqual("(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi");
		expect(timezoneOptions[1].innerText.trim()).toEqual("(UTC-05:00) Estern Time");
	});

	it('should expose default timezone to be currentUserInfo timezone', function () {
		var panel = setUp();
		var selectedOption = panel[0].querySelector('md-option[selected] .md-text');
		expect(selectedOption.innerText).toEqual('(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi');
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


	function setUp(timezone, availableTimezones) {
		var scope = $rootScope.$new();
		var html = '<timezone-picker class="timezone-picker" ng-model="timezone" available-timezones="availableTimezones"></timezone-picker> ';
		scope.availableTimezones = availableTimezones || [
			{
				IanaId: "Asia/Hong_Kong",
				DisplayName: "(UTC+08:00) Beijing, Chongqing, Hong Kong, Urumqi"
			}, {
				IanaId: "Europe/Berlin",
				DisplayName: "(UTC+01:00) Amsterdam, Berlin, Bern, Rome, Stockholm, Vienna"
			}
		];
		scope.timezone = timezone;

		var element = $compile(html)(scope);
		scope.$apply();

		return element;

	}

});