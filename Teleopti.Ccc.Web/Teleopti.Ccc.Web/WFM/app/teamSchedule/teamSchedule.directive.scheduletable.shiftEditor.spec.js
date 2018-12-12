(function () {
	var $compile,
		$rootScope,
		$timeout,
		scheduleManagement;

	describe("<shift-editor>", function () {
		beforeEach(function () {
			module('wfm.templates');
			module("wfm.teamSchedule");
		});
		beforeEach(
			module(function ($provide) {
				$provide.service('Toggle', function () {
					return { WfmTeamSchedule_DisplaySchedulesInShiftEditor_75978: true };
				});
				
				$provide.service('ActivityService', function () {
					fakeActivityService = new FakeActivityService();
					return fakeActivityService;
				});
			
				fakeTeamSchedule = new FakeTeamSchedule();
				$provide.service('TeamSchedule', function () {
					return fakeTeamSchedule;
				});

				fakeNoticeService = new FakeNoticeService();
				$provide.service('NoticeService', function () {
					return fakeNoticeService;
				});
			})
		);

		beforeEach(inject(function (_$rootScope_, _$compile_, _$timeout_, ScheduleManagement) {
			$compile = _$compile_;
			$rootScope = _$rootScope_;
			$timeout = _$timeout_;
			scheduleManagement = ScheduleManagement;
		}));


		xit('should show shift editor view after click the edit button', function () {
			var schedule = {
				PersonId: 'e0e171ad-8f81-44ac-b82e-9c0f00aa6f22',
				Name: 'Annika Andersson',
				Date: '2018-12-12',
				WorkTimeMinutes: 240,
				ContractTimeMinutes: 240,
				Projection: [
					{
						ShiftLayerIds: ['61678e5a-ac3f-4daa-9577-a83800e49622'],
						Color: '#ffffff',
						Description: 'E-mail',
						Start: '2018-12-12 08:00',
						Minutes: 120,
						IsOvertime: false
					}
				],
				Timezone: { IanaId: 'Europe/Berlin' }
			};
			fakeTeamSchedule.has(schedule);

			scheduleManagement.resetSchedules([schedule], '2018-12-12', 'Europe/Berlin');

			var container = setUp('2018-12-12', 'Europe/Berlin');
			container[0].querySelector('.editor').click();

			expect(!!container[0].querySelector('shift-editor')).toBeTruthy();
		});

		function setUp(selectedDate, selectedTimezone) {
			var html = '<schedule-table select-mode="true" selected-date="selectedDate" selected-timezone="selectedTimezone"></schedule-table>';
			var scope = $rootScope.$new();
			scope.selectedDate = selectedDate;
			scope.selectedTimezone = selectedTimezone;
			var container = $compile(html)(scope);
			scope.$apply();
			return container;
		}

		function FakeActivityService() {
			var activities = [
				{
					Id: '472e02c8-1a84-4064-9a3b-9b5e015ab3c6',
					Name: 'E-mail',
					Color: '#FFa2a2',
					FloatOnTop: false
				},
				{
					Id: '5c1409de-a0f1-4cd4-b383-9b5e015ab3c6',
					Name: 'Invoice',
					Color: '#FF0000',
					FloatOnTop: false
				},
				{
					Id: '0ffeb898-11bf-43fc-8104-9b5e015ab3c2',
					Name: 'Phone',
					Color: '#ffffff',
					FloatOnTop: false
				},
				{
					Id: '84db44f4-22a8-44c7-b376-a0a200da613e',
					Name: 'Sales',
					Color: '#FFCCA2',
					FloatOnTop: false
				},
				{
					Id: '35e33821-862f-461c-92db-9f0800a8d095',
					Name: 'Social Media',
					Color: '#FFA2CC',
					FloatOnTop: false
				},
				{
					Id: '12e33821-862f-461c-92db-9f0800a8d056',
					Name: 'Lunch',
					Color: '#ffff00',
					FloatOnTop: true
				},
				{
					Id: 'sbs33821-862f-461c-92db-9f0800a8d056',
					Name: 'Short Break',
					Color: '#ff0500',
					FloatOnTop: true
				}
			];
			this.fetchAvailableActivities = function () {
				return {
					then: function (callback) {
						callback(activities);
					}
				};
			};
		}

		function FakeTeamSchedule() {
			var self = this;
			self.schedules = [];
			this.has = function (schedule) {
				self.schedules = [schedule];
			};
			this.getSchedules = function () {
				return {
					then: function (callback) {
						callback({ Schedules: self.schedules });
					}
				};
			};
		}

		function FakeNoticeService() {
			this.successMessage = '';
			this.errorMessage = '';
			this.warningMessage = '';
			this.success = function (message, time, destroyOnStateChange) {
				this.successMessage = message;
			};
			this.error = function (message, time, destroyOnStateChange) {
				this.errorMessage = message;
			};
			this.warning = function (message, time, destroyOnStateChange) {
				this.warningMessage = message;
			};
		}

	});
})();