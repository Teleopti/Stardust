describe('<shift-editor>',
function () {
		'use strict';

		var $rootScope,
			$compile,
			$document;

		beforeEach(module('wfm.templates', 'wfm.teamSchedule'));
		beforeEach(inject(function (_$rootScope_, _$compile_, _$document_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
			$document = _$document_;
		}));
		beforeEach(function () { moment.locale('sv'); });
		afterEach(function () { moment.locale('en'); });

		it("should render correctly", function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-16'
			}, '2018-05-16');
			var element = panel[0];

			expect(element.querySelector('.name').innerText.trim()).toEqual('Agent 1');
			expect(element.querySelector('.date').innerText).toEqual('2018-05-16');
		});

		it('should show underlying info icon if schedule has underlying activities', function () {
			var scheduleDate = "2018-05-16";
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-16',
				UnderlyingScheduleSummary: {
					"PersonalActivities": [{
						"Description": "personal activity",
						"Start": scheduleDate + ' 10:00',
						"End": scheduleDate + ' 11:00'
					}]
				},
				HasUnderlyingSchedules: function () { return true; }
			}, '2018-05-16');

			var element = panel[0];
			expect(element.querySelectorAll('.underlying-info').length).toBe(1);
		});

		it('should render time line correctly', function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			}, "2018-05-21", "Europe/Berlin");
			var timeline = panel[0].querySelector(".timeline");
			var intervals = panel[0].querySelectorAll(".timeline .interval");
			var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
			var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
			expect(!!timeline).toBeTruthy();
			expect(intervals.length).toBe(37);
			expect(textHours).toEqual( ["00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
			expect(intervals[0].querySelectorAll('.tick').length).toBe(12);
			expect(intervals[0].querySelectorAll('.tick.hour').length).toBe(1);
			expect(intervals[0].querySelectorAll('.tick.half-hour').length).toBe(1);
			expect(intervals[intervals.length - 1].querySelectorAll('.tick').length).toBe(1);
		});

		it('should render time line correctly on DST', function () {
			var panel = setUp({
				Name: 'Agent 1',
				Date: '2018-03-25',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			}, "2018-03-25", "Europe/Berlin");
			var timeline = panel[0].querySelector(".timeline");
			var intervals = panel[0].querySelectorAll(".timeline .interval");
			var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
			var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
			expect(!!timeline).toBeTruthy();
			expect(intervals.length).toBe(36);
			expect(textHours).toEqual(["00:00", "01:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00", "13:00", "14:00", "15:00", "16:00", "17:00", "18:00", "19:00", "20:00", "21:00", "22:00", "23:00", "00:00", "01:00", "02:00", "03:00", "04:00", "05:00", "06:00", "07:00", "08:00", "09:00", "10:00", "11:00", "12:00"]);
		});

		it('should show all shifts correctly', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 07:30",
						Minutes: 60
					}, {
						Color: "#000000",
						Description: "Email",
						Start: "2018-05-21 08:30",
						Minutes: 30,
						IsOvertime: true,
						UseLighterBorder: true
					},
					{
						Color: "#000000",
						Description: "Email",
						Start: "2018-05-21 09:00",
						Minutes: 30,
						IsOvertime: true,
						UseLighterBorder: false
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");
			var schedulesEl = panel[0].querySelector(".shift");
			var shiftLayers = schedulesEl.querySelectorAll(".shift-layer");
			expect(shiftLayers.length).toEqual(4);
			expect(shiftLayers[0].style.width).toEqual('120px');
			expect(shiftLayers[1].style.width).toEqual('60px');
			expect(shiftLayers[2].style.width).toEqual('30px');
			expect(shiftLayers[3].style.width).toEqual('30px');

			expect(shiftLayers[0].style.left).toEqual('330px');
			expect(shiftLayers[1].style.left).toEqual('450px');
			expect(shiftLayers[2].style.left).toEqual('510px');
			expect(shiftLayers[3].style.left).toEqual('540px');

			expect(angular.element(shiftLayers[2]).hasClass('overtime')).toBeTruthy();
			expect(angular.element(shiftLayers[2]).hasClass('overtime-light')).toBeTruthy();
			expect(angular.element(shiftLayers[3]).hasClass('overtime')).toBeTruthy();
			expect(angular.element(shiftLayers[3]).hasClass('overtime-dark')).toBeTruthy();

		});

		it('should show all shifts for schedule on DST', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-03-25',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 05:00",
						Minutes: 120
					}]
				}]
			};
			var panel = setUp(schedule, "2018-03-25", "Europe/Berlin");
			var schedulesEl = panel[0].querySelector(".shift");
			var shiftLayers = schedulesEl.querySelectorAll(".shift-layer");
			expect(shiftLayers.length).toEqual(1);
			expect(shiftLayers[0].style.width).toEqual('120px');
			expect(shiftLayers[0].style.left).toEqual('240px');
		});

		it('should able to select an activity', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 07:30",
						Minutes: 60
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

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
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 07:30",
						Minutes: 60,
						ShiftLayerIds: ['e2964c98-b3e7-4bad-8d9a-a7bc00f9f8e9']
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			shiftLayers[1].click();

			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
			expect(shiftLayers[1].className.indexOf('selected') >= 0).toBeTruthy();
		});

		it('should clear selection when click the selected activity again', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			shiftLayers[0].click();

			expect(shiftLayers[0].className.indexOf('selected') >= 0).toBeFalsy();
			var activityInfoEl = panel[0].querySelector(".activity-info");
			expect(!!activityInfoEl).toBeFalsy();
		});

		it('should show border color correctly based on the selected activity color', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-05-21',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 05:30",
						Minutes: 120,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9'],
						UseLighterBorder: true
					}, {
						Color: "#FFC080",
						Description: "Sales",
						Start: "2018-05-21 07:30",
						Minutes: 60,
						ShiftLayerIds: ['e2964c98-b3e7-4bad-8d9a-a7bc00f9f8e9'],
						UseLighterBorder: false
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();
			expect(shiftLayers[0].className.indexOf('border-light') >= 0).toBeTruthy();
			shiftLayers[1].click();
			expect(shiftLayers[1].className.indexOf('border-dark') >= 0).toBeTruthy();
		});

		it('should show activity information correctly when select an activity', function () {
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
						Minutes: 60,
						ShiftLayerIds: ['e2964c98-b3e7-4bad-8d9a-a7bc00f9f8e9']
					},
					{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-05-21 10:00",
						Minutes: 60,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}]
				}]
			};
			var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector(".timespan");
			var typeEl = panel[0].querySelector(".activity-type");

			expect(timespanEl.innerText.trim()).toBe("08:00 - 09:00");
			expect(typeEl.innerText.trim()).toBe("Phone");

		});

		it('should show time period of activity information correctly on DST when select an activity', function () {
			var schedule = {
				Name: 'Agent 1',
				Date: '2018-03-25',
				Shifts: [{
					Projections: [{
						Color: "#80FF80",
						Description: "Phone",
						Start: "2018-03-25 00:30",
						Minutes: 120,
						ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
					}]
				}]
			};
			var panel = setUp(schedule, "2018-03-25", "Europe/Berlin");

			var shiftLayers = panel[0].querySelectorAll(".shift-layer");
			shiftLayers[0].click();

			var timespanEl = panel[0].querySelector(".timespan");
			expect(timespanEl.innerText.trim()).toBe("00:30 - 03:30");
		});

		describe("in locale en-UK", function () {
			beforeEach(function () { moment.locale('en-UK'); });
			afterEach(function () { moment.locale('en'); });

			it('should render time line correctly based on current user locale', function () {
				var panel = setUp({
					Name: 'Agent 1',
					Date: '2018-05-21',
					Shifts: [{
						Projections: [{
							Color: "#80FF80",
							Description: "Phone",
							Start: "2018-03-25 05:00",
							Minutes: 120
						}]
					}]
				}, "2018-05-21", "Europe/Berlin");
				var timeline = panel[0].querySelector(".timeline");
				var intervals = panel[0].querySelectorAll(".timeline .interval");
				var hours = panel[0].querySelectorAll(".timeline .interval .label>span");
				var textHours = [].slice.call(hours).map(function (hourEl) { return hourEl.innerText.trim() });
				expect(!!timeline).toBeTruthy();
				expect(intervals.length).toBe(37);
				expect(textHours).toEqual( ["12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM", "1:00 PM", "2:00 PM", "3:00 PM", "4:00 PM", "5:00 PM", "6:00 PM", "7:00 PM", "8:00 PM", "9:00 PM", "10:00 PM", "11:00 PM", "12:00 AM", "1:00 AM", "2:00 AM", "3:00 AM", "4:00 AM", "5:00 AM", "6:00 AM", "7:00 AM", "8:00 AM", "9:00 AM", "10:00 AM", "11:00 AM", "12:00 PM"]);
			});

			it('should show time period of activity information correctly when select an activity', function () {
				var schedule = {
					Name: 'Agent 1',
					Date: '2018-05-21',
					Shifts: [{
						Projections: [{
							Color: "#80FF80",
							Description: "Phone",
							Start: "2018-05-21 05:30",
							Minutes: 120,
							ShiftLayerIds: ['9be3096b-d989-4821-9d32-a7bc00f9f8e9']
						}]
					}]
				};
				var panel = setUp(schedule, "2018-05-21", "Europe/Berlin");

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
				var panel = setUp({
					Name: 'Agent 1',
					Date: '2018-05-24'
				}, '2018-05-24');
				var element = panel[0];

				expect(element.querySelector('.date').innerText).toEqual('۲۰۱۸-۰۵-۲۴');
			});
		});

		function setUp(personSchedule, date, timezone) {
			var scope = $rootScope.$new();
			scope.personSchedule = personSchedule;
			scope.date = date;
			scope.timezone = timezone;
			var element = $compile('<shift-editor person-schedule="personSchedule" date="date" timezone="timezone"></shift-editor>')(scope);
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
				personId: '9be3096b-d989-4821-9d32-a7bc00f9f8e9',
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
		expect(target.personId).toEqual('9be3096b-d989-4821-9d32-a7bc00f9f8e9');
		expect(target.timezone).toEqual('Europe/Berlin');
		expect(target.date).toEqual('2018-05-28');
		expect(target.schedules[0]).toEqual(schedule);
	});

	it('should get schedule when schedule was updated', function () {
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

		expect(target.schedules[1]).toBe(updateSchedule);
	})

	function setUp() {
		return $controller("ShiftEditorViewController", { TeamSchedule: fakeTeamSchedule });
	}

	function FakeTeamSchedule() {
		var schedules = [];
		this.has = function (schedule) {
			schedules.push(schedule);
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
			subscribe: function (options, eventHandler, errorHandler) {
				mockSignalRBackendServer.subscriptions.push(options);
				mockSignalRBackendServer.notifyClients = eventHandler;
			}
		};
	}
});




