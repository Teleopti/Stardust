'use strict';
(function() {
	describe('<requests-shift-detail>', function() {
		var $rootScope, $compile, fakeTeamSchedule, fakeCurrentUserInfo, fakeShiftTradeScheduleService, fakeToggleService;
		beforeEach(function() {
			module('wfm.templates');
			module('wfm.requests');
		});

		beforeEach(function() {
			fakeTeamSchedule = new FakeTeamSchedule();
			fakeCurrentUserInfo = new FakeCurrentUserInfo();
			fakeToggleService = new FakeToggleService();
			fakeShiftTradeScheduleService = new FakeShiftTradeScheduleService();
			module(function($provide) {
				$provide.service('Toggle', function () {
					return fakeToggleService;
				});
				$provide.service('TeamSchedule', function() {
					return fakeTeamSchedule;
				});
				$provide.service('CurrentUserInfo', function() {
					return fakeCurrentUserInfo;
				});
				$provide.service('shiftTradeScheduleService', function () {
					return fakeShiftTradeScheduleService;
				});
			});
		});

		beforeEach(inject(function(_$rootScope_, _$compile_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
		}));
		
		it('should show the time of shift detail in current time zone', function () {
			fakeTeamSchedule.setSchedules([
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Date: '2018-10-24',
					ShiftCategory: {
						ShortName: 'AM',
						Name: 'Early',
						DisplayColor: '#000000'
					},
					Projection: [
						{
							ShiftLayerIds: ['31ffe214-3384-4a80-a14c-a83800e23276'],
							Color: '#795548',
							Description: 'Phone',
							StartInUtc: '2018-10-24 08:00',
							EndInUtc: '2018-10-24 10:00',
							IsOvertime: false
						}
					],
					DayOff: null
				},
				{
					PersonId: 'agent2',
					Name: 'agent1',
					Date: '2018-10-24',
					ShiftCategory: {
						ShortName: 'AM',
						Name: 'Early',
						DisplayColor: '#000000'
					},
					Projection: [
						{
							ShiftLayerIds: ['31ffe214-3384-4a80-a14c-a83800e23276'],
							Color: '#fff000',
							Description: 'Email',
							StartInUtc: '2018-10-24 08:00',
							EndInUtc: '2018-10-24 10:00',
							IsOvertime: false
						}
					],
					DayOff: null
				}
			]);

			var element = setUp(['agent1', 'agent2'], '2018-10-24', 'Etc/UTC', function(params) {
				var schedules = params.schedules.Schedules;
				expect(schedules[0].Shifts[0].Projections[0].TimeSpan).toEqual('8:00 AM - 10:00 AM');
				expect(schedules[1].Shifts[0].Projections[0].TimeSpan).toEqual('8:00 AM - 10:00 AM');
			});

			element[0].click();
		});

		it('should show the time of shift detail in target time zone if target time zone is different from current timezone', function() {
			fakeTeamSchedule.setSchedules([
				{
					PersonId: 'agent1',
					Name: 'agent1',
					Date: '2018-10-24',
					ShiftCategory: {
						ShortName: 'AM',
						Name: 'Early',
						DisplayColor: '#000000'
					},
					Projection: [
						{
							ShiftLayerIds: ['31ffe214-3384-4a80-a14c-a83800e23276'],
							Color: '#795548',
							Description: 'Phone',
							StartInUtc: '2018-10-24 08:00',
							EndInUtc: '2018-10-24 10:00',
							IsOvertime: false
						}
					],
					DayOff: null
				},
				{
					PersonId: 'agent2',
					Name: 'agent1',
					Date: '2018-10-24',
					ShiftCategory: {
						ShortName: 'AM',
						Name: 'Early',
						DisplayColor: '#000000'
					},
					Projection: [
						{
							ShiftLayerIds: ['31ffe214-3384-4a80-a14c-a83800e23276'],
							Color: '#fff000',
							Description: 'Email',
							StartInUtc: '2018-10-24 08:00',
							EndInUtc: '2018-10-24 10:00',
							IsOvertime: false
						}
					],
					DayOff: null
				}
			]);

			var element = setUp(['agent1', 'agent2'], '2018-10-24', 'Europe/Berlin', function(params) {
				var schedules = params.schedules.Schedules;
				expect(schedules[0].Shifts[0].Projections[0].TimeSpan).toEqual('10:00 AM - 12:00 PM');
				expect(schedules[1].Shifts[0].Projections[0].TimeSpan).toEqual('10:00 AM - 12:00 PM');
			});

			element[0].click();
		});

		it('should call ShiftTradeScheduleService when Toggle WFM_Request_Show_Shift_for_ShiftTrade_Requests_79412 is ON', function () {

			fakeToggleService.WFM_Request_Show_Shift_for_ShiftTrade_Requests_79412 = true;

			fakeShiftTradeScheduleService.setSchedules(
				{
					Name: 'agent3',
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					BelongsToDate: '2018-10-24'
				},
				{
					Name: 'agent4',
					IsDayOff: false,
					DayOffName: null,
					IsNotScheduled: false,
					BelongsToDate: '2018-10-24'
				},
				null
			);

			var element = setUp(['agent3', 'agent4'], '2018-10-24', 'Europe/Berlin', function (params) {
				var personFromSchedule = params.schedules.PersonFromSchedule;
				expect(params.schedules.PersonFromSchedule.Name).toEqual('agent3');
				expect(params.schedules.PersonToSchedule.Name).toEqual('agent4');
			});

			element[0].click();
		});
		


		function setUp(personIds, date, targetTimezone, showShiftDetail) {
			var scope = $rootScope.$new();
			scope.personIds = personIds;
			scope.date = date;
			scope.targetTimezone = targetTimezone;
			scope.showShiftDetail = showShiftDetail;

			var element = $compile(
				'<div requests-shift-detail date="date" target-timezone="targetTimezone" person-ids="personIds" show-shift-detail="showShiftDetail(params)"></div>'
			)(scope);
			scope.$apply();

			return element;
		}

		function FakeToggleService() {
			this.WFM_Request_Show_Shift_for_ShiftTrade_Requests_79412 = false;
		}

		function FakeTeamSchedule() {
			var schedules = [];

			this.setSchedules = function(items) {
				schedules = items;
			};

			this.getSchedules = function(date, agents) {
				return {
					then: function(cb) {
						cb({ Schedules: schedules });
					}
				};
			};
		}

		function FakeShiftTradeScheduleService() {
			
			var timeLine = [];
			var personFromSchedule = {};
			var personToSchedule = {};
			this.setSchedules = function (_personFromSchedule, _personToSchedule, _timeLine) {
				personFromSchedule = _personFromSchedule;
				personToSchedule = _personToSchedule;
				timeLine = _timeLine;
			};
			this.getSchedules = function (date, personFromId, personToId) {
				return {
					then: function (cb) {
						cb({ TimeLine: timeLine, PersonFromSchedule: personFromSchedule, PersonToSchedule: personToSchedule });
					}							  
				};
			}
		}


		function FakeCurrentUserInfo() {
			this.CurrentUserInfo = function() {
				return {
					DefaultTimeZone: 'Etc/UTC',
					DefaultTimeZoneName: 'Etc/UTC',
					DateFormatLocale: 'en-GB'
				};
			};
		}
	});
})();
