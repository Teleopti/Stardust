'use strict';
(function() {
	describe('<requests-shift-detail>', function() {
		var $rootScope, $compile, fakeTeamSchedule, fakeCurrentUserInfo;
		beforeEach(function() {
			module('wfm.templates');
			module('wfm.requests');
		});

		beforeEach(function() {
			fakeTeamSchedule = new FakeTeamSchedule();
			fakeCurrentUserInfo = new FakeCurrentUserInfo();
			module(function($provide) {
				$provide.service('Toggle', function() {});
				$provide.service('TeamSchedule', function() {
					return fakeTeamSchedule;
				});
				$provide.service('CurrentUserInfo', function() {
					return fakeCurrentUserInfo;
				});
			});
		});

		beforeEach(inject(function(_$rootScope_, _$compile_) {
			$rootScope = _$rootScope_;
			$compile = _$compile_;
		}));

		it('should show the time of shift detail in current time zone', function() {
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
