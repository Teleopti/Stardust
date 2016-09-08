(function() {
	'use strict';

	describe('teamScheduleNotificationService test', function() {
		var target, fakeNotice;
		beforeEach(function () {
			module("wfm.teamSchedule");
		});
		beforeEach(function () {
			module(function ($provide) {
				$provide.service('NoticeService', function () {
					fakeNotice = new FakeNotice();
					return fakeNotice;
				});
			});

		});
		beforeEach(inject(function(teamScheduleNotificationService) {
			target = teamScheduleNotificationService;
		}));


		it('should notify warning when action result contains warnings', function() {
			var actionResults = [
				{
					PersonId: 'testPerson1',
					WarningMessages: ['warning1']
				}
			];
			var actionTargets = [
				{
					PersonId: 'testPerson1',
					Name: 'testPerson1'
				}
			];
			var commandTemplates = {
				success: 'success',
				warning: 'partial success'
			};
			target.reportActionResult(commandTemplates, actionTargets, actionResults);

			expect(fakeNotice.getLastWarning()).toBe('warning1 : testPerson1');

		});

		it('should not notify warning when action result not contains warnings', function () {
			fakeNotice.reset();
			var actionResults = [
				{
					PersonId: 'testPerson1',
					ErrorMessages: ['error happens']
				}
			];
			var actionTargets = [
				{
					PersonId: 'testPerson1',
					Name: 'testPerson1'
				}
			];
			var commandTemplates = {
				success: 'success',
				warning: 'partial success'
			};
			target.reportActionResult(commandTemplates, actionTargets, actionResults);

			expect(fakeNotice.getLastWarning()).toBe('partial success');

		});

		function FakeNotice() {
			var warning = '';
			var error = '';
			var success = '';
			this.warning = function (message, time, destroyOnStateChange) {
				warning = message;
			}
			this.success = function (message, time, destroyOnStateChange) {
				success = message;
			}
			this.error = function (message, time, destroyOnStateChange) {
				error = message;
			}
			this.reset = function () {
				warning = '';
				error = '';
				success = '';
			}
			this.getLastWarning = function () {
				return warning;
			}
		};
	});
})();
