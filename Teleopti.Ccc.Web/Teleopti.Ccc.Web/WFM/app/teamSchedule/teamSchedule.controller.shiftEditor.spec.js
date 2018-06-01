describe('<shift-editor>',
	function () {
		'use strict';

		var $rootScope,
			$compile,
			$document;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
		beforeEach(module(function ($provide) {
			$provide.service('Toggle', function () { return { WfmTeamSchedule_ShowInformationForUnderlyingSchedule_74952: true } });
			$provide.service('CurrentUserInfo', function () {
				return {
					CurrentUserInfo: function () {
						return {
							DefaultTimeZone: "Europe/Berlin",
							DateFormatLocale: "sv-SE"
						};
					}
				};
			});
		}));
		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
		}));
		beforeEach(function () { moment.locale('sv'); });
		afterEach(function () { moment.locale('en'); });

		it("should render correctly", function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "E-mail",
					"Start": "2018-05-28 08:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', [schedule], '2018-05-28', 'Europe/Berlin');
			var element = panel[0];

			expect(element.querySelector('.name').innerText.trim()).toEqual('Annika Andersson');
			expect(element.querySelector('.date').innerText).toEqual('2018-05-28');
		});

		it('should show underlying info icon if schedule has underlying activities', function () {
			var scheduleDate = "2018-05-16";
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [{
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				Name: 'Agent 1',
				Date: '2018-05-16',
				UnderlyingScheduleSummary: {
					"PersonalActivities": [{
						"Description": "personal activity",
						"Start": scheduleDate + ' 10:00',
						"End": scheduleDate + ' 11:00'
					}]
				}
			}], '2018-05-16', "Europe/Berlin");

			var element = panel[0];
			expect(element.querySelectorAll('.underlying-info').length).toBe(1);
		});

		it('should able to select an activity', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "E-mail",
					"Start": "2018-05-28 08:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-28", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeTruthy();
		});

		xit('should select the whole activity', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 08:00",
						Minutes: 60,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 09:00",
						Minutes: 60
					},
					{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 10:00",
						Minutes: 180,
						ShiftLayerIds: ['e2964c98-b3e7-4bad-8d9a-a7bc00f9f8e9', '9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");

			shiftLayers[0].click();
			expect(shiftLayers[0].className.indexOf("selected") >= 0).toBeTruthy();
			expect(shiftLayers[2].className.indexOf("selected") >= 0).toBeTruthy();

		});

		it('should can select only one activity', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "E-mail",
					"Start": "2018-05-28 08:00",
					"Minutes": 120,
					"IsOvertime": false
				}, {
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49623"],
					"Color": "#808080",
					"Description": "Phone",
					"Start": "2018-05-28 10:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-28", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			shiftLayers[1].click();

			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeTruthy();
		});

		it('should clear selection when click the selected activity again', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "E-mail",
					"Start": "2018-05-28 08:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-28", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			shiftLayers[0].click();

			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
			var activityInfoEl = panel[0].querySelector(".activity-info");
			expect(!!activityInfoEl).toBeFalsy();
		});

		it('should show border color correctly based on the selected activity color', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "E-mail",
					"Start": "2018-05-28 08:00",
					"Minutes": 120,
					"IsOvertime": false
				}, {
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49623"],
					"Color": "#8080c0",
					"Description": "Phone",
					"Start": "2018-05-28 10:00",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-28", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			expect(shiftLayers[0].className.indexOf('border-dark') >= 0).toBeTruthy();
			shiftLayers[1].click();
			expect(shiftLayers[1].className.indexOf('border-light') >= 0).toBeTruthy();
		});

		it('should show activity information correctly when select an activity', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-05-28",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "Phone",
					"Start": "2018-05-28 08:00",
					"Minutes": 60,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector(".timespan");
			var typeEl = panel[0].querySelector(".activity-type");

			expect(timespanEl.innerText.trim()).toBe("08:00 - 09:00");
			expect(typeEl.innerText.trim()).toBe("Phone");

		});

		it('should show time period of activity information correctly on DST when select an activity', function () {
			var schedule = {
				"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"Name": "Annika Andersson",
				"Date": "2018-03-25",
				"WorkTimeMinutes": 240,
				"ContractTimeMinutes": 240,
				"Projection": [{
					"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
					"Color": "#ffffff",
					"Description": "Phone",
					"Start": "2018-03-25 00:30",
					"Minutes": 120,
					"IsOvertime": false
				}],
				"Timezone": { "IanaId": "Europe/Berlin" }
			};
			var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-03-25", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector(".timespan");
			expect(timespanEl.innerText.trim()).toBe("00:30 - 03:30");
		});

		describe("in locale en-UK", function () {
			beforeEach(function () { moment.locale('en-UK'); });
			afterEach(function () { moment.locale('en'); });

			it('should show time period of activity information correctly when select an activity', function () {
				var schedule = {
					"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
					"Name": "Annika Andersson",
					"Date": "2018-05-21",
					"WorkTimeMinutes": 240,
					"ContractTimeMinutes": 240,
					"Projection": [{
						"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
						"Color": "#ffffff",
						"Description": "Phone",
						"Start": "2018-05-21 05:30",
						"Minutes": 120,
						"IsOvertime": false
					}],
					"Timezone": { "IanaId": "Europe/Berlin" }
				};
				var panel = setUp("e0e171ad-8f81-44ac-b82e-9c0f00aa6f22", [schedule], "2018-05-21", "Europe/Berlin");

				var shiftLayers = panel[0].querySelectorAll(".shift-layer");
				shiftLayers[0].click();

				var timespanEl = panel[0].querySelector(".timespan");
				expect(timespanEl.innerText.trim()).toBe("5:30 AM - 7:30 AM");

			});
		});

		describe("in locale fa", function () {
			beforeEach(function () { moment.locale('fa'); });
			afterEach(function () { moment.locale('en'); });

			it("should render schedule date correctly", function () {
				var schedule = {
					"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
					"Name": "Annika Andersson",
					"Date": "2018-05-24",
					"WorkTimeMinutes": 240,
					"ContractTimeMinutes": 240,
					"Projection": [{
						"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
						"Color": "#ffffff",
						"Description": "E-mail",
						"Start": "2018-05-24 08:00",
						"Minutes": 120,
						"IsOvertime": false
					}],
					"Timezone": { "IanaId": "Europe/Berlin" }
				};
				var panel = setUp('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22', [schedule], '2018-05-24', 'Europe/Berlin');
				var element = panel[0];

				expect(element.querySelector('.date').innerText).toEqual('۲۰۱۸-۰۵-۲۴');
			});
		});

		function setUp(personId, schedules, date, timezone) {
			var scope = $rootScope.$new();
			scope.personId = personId;
			scope.schedules = schedules;
			scope.date = date;
			scope.timezone = timezone;

			var element = $compile('<shift-editor schedules="schedules" date="date" timezone="timezone" person-id="personId"></shift-editor>')(scope);
			scope.$apply();
			return element;
		}

	});

describe('#shiftEditorController#', function () {
	var $controller, stateParams, fakeTeamSchedule, mockSignalRBackendServer = {};
	beforeEach(function () {
		module('wfm.teamSchedule');
		module(function ($provide) {
			stateParams = {
				personId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				timezone: 'Europe/Berlin',
				date: '2018-05-28'
			};

			$provide.service('$stateParams', function () { return stateParams; });
			$provide.service('signalRSVC', setupMockSignalRService);
		});
	});
	beforeEach(inject(function (_$controller_) {
		$controller = _$controller_;
		fakeTeamSchedule = new FakeTeamSchedule();
	}));

	it('should set up correctly and get schedule from state params', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#8080c0",
				"Description": "E-mail",
				"Start": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(schedule);

		var target = setUp();
		expect(target.personId).toEqual('e0e171ad-8f81-44ac-b82e-9c0f00aa6f22');
		expect(target.timezone).toEqual('Europe/Berlin');
		expect(target.date).toEqual('2018-05-28');
		expect(target.schedules[0]).toEqual(schedule);
	});

	it('should get schedulewhen schedule was updated', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#8080c0",
				"Description": "E-mail",
				"Start": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(schedule);
		var target = setUp();

		var updateSchedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-05-28 09:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(updateSchedule);

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-05-28T00:00:00",
				"EndDate": "D2018-05-28T00:00:00"
			}
		]);

		expect(target.schedules[0]).toBe(updateSchedule);
	});


	it('should only get schedule for same person and same date when schedule was updated', function () {
		var schedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			},
			{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#8080c0",
				"Description": "E-mail",
				"Start": "2018-05-28 10:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(schedule);

		var target = setUp();

		var scheduleForAnotherAgent = {
			"PersonId": "b0e171ad-8f81-44ac-b82e-9c0f00aa6f23",
			"Name": "Annika Andersson",
			"Date": "2018-05-27",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["31678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "Phone",
				"Start": "2018-05-27 09:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(scheduleForAnotherAgent);

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "b0e171ad-8f81-44ac-b82e-9c0f00aa6f23",
				"StartDate": "D2018-05-27T00:00:00",
				"EndDate": "D2018-05-27T00:00:00"
			}
		]);

		expect(target.schedules[0]).toBe(schedule);

		var newSchedule = {
			"PersonId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
			"Name": "Annika Andersson",
			"Date": "2018-05-28",
			"WorkTimeMinutes": 240,
			"ContractTimeMinutes": 240,
			"Projection": [{
				"ShiftLayerIds": ["61678e5a-ac3f-4daa-9577-a83800e49622"],
				"Color": "#ffffff",
				"Description": "E-mail",
				"Start": "2018-05-28 08:00",
				"Minutes": 120,
				"IsOvertime": false
			}],
			"Timezone": { "IanaId": "Europe/Berlin" }
		};
		fakeTeamSchedule.has(newSchedule);

		mockSignalRBackendServer.notifyClients([
			{
				"DomainReferenceId": "e0e171ad-8f81-44ac-b82e-9c0f00aa6f22",
				"StartDate": "D2018-05-28T00:00:00",
				"EndDate": "D2018-05-28T00:00:00"
			}
		]);

		expect(target.schedules[0]).toBe(newSchedule);
	});

	function setUp() {
		return $controller("ShiftEditorViewController", { TeamSchedule: fakeTeamSchedule });
	}

	function FakeTeamSchedule() {
		var schedules = [];
		this.has = function (schedule) {
			if (!schedules.length || (schedules[0].PersonId == schedule.PersonId && schedules[0].Date == schedule.Date)) {
				schedules = [];
				schedules.push(schedule);
			}
			
		}
		this.getSchedules = function () {
			return {
				then: function (callback) {
					callback({ Schedules: schedules });
				}
			};

		}
	}

	function setupMockSignalRService() {
		mockSignalRBackendServer.subscriptions = [];

		return {
			subscribeBatchMessage: function (options, messageHandler, timeout) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = messageHandler;
			}
		};
	}
});
