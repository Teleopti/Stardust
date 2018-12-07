(function () {
	'use strict';
	describe('#export schedule service#', function () {
		var $httpBackend,
			exportScheduleService;

		beforeEach(module('wfm.teamSchedule'));

		beforeEach(inject(function (_$httpBackend_, _exportScheduleService_) {
			$httpBackend = _$httpBackend_;
			exportScheduleService = _exportScheduleService_;
		}));

		function commonTestsInDifferentLocale() {
			it('should start export with correct normalized data', function () {
				var response = null;
				exportScheduleService.startExport({
					period: {
						startDate: '2018-02-24',
						endDate: '2018-02-24'
					},
					selectedGroups: { groupIds: undefined, groupPageId: 'group page' },
					timezoneId: 'timezone',
					scenarioId: 'scenario',
					optionalColumnIds: ['option columns']
				});

				$httpBackend.expect('POST', '../api/TeamSchedule/StartExport', {
					StartDate: '2018-02-24',
					EndDate: '2018-02-24',
					ScenarioId: 'scenario',
					TimezoneId: 'timezone',
					SelectedGroups: { SelectedGroupPageId: 'group page' },
					OptionalColumnIds: ['option columns']

				}).respond(200, response = { success: true });

				$httpBackend.flush();
			});
		}

		commonTestsInDifferentLocale();

		describe('in locale ar-AE', function () {
			beforeAll(function () {
				moment.locale('ar-AE');
			});

			afterAll(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});

		describe('in locale fa-IR', function () {
			beforeAll(function () {
				moment.locale('fa-IR');
			});

			afterAll(function () {
				moment.locale('en');
			});

			commonTestsInDifferentLocale();
		});
	});
})();
