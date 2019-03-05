'use strict';
(function() {
	describe('<requests-shift-detail>', function() {
		var $rootScope, $compile, fakeTeamSchedule, fakeCurrentUserInfo, fakeShiftTradeScheduleService;
		beforeEach(function() {
			module('wfm.templates');
			module('wfm.requests');
		});

		beforeEach(function() {
			fakeTeamSchedule = new FakeTeamSchedule();
			fakeCurrentUserInfo = new FakeCurrentUserInfo();
			fakeShiftTradeScheduleService = new FakeShiftTradeScheduleService();
			module(function ($provide) {
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

		it('should call ShiftTradeScheduleService', function () {
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
