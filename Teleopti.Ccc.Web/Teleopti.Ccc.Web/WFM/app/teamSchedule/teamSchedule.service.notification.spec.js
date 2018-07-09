(function () {
	'use strict';

	describe('teamScheduleNotificationService test', function () {
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
		beforeEach(inject(function (teamScheduleNotificationService) {
			target = teamScheduleNotificationService;
		}));


		it('should notify warning when action result contains warnings', function () {
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
			expect(fakeNotice.calledCountForWarning).toBe(1);
		});

		it('should not notify warning when action result contains WarningMessages and command info not include warning object', function () {
			var actionResults = [
				{
					PersonId: 'testPerson1',
					ErrorMessages: ['error happens'],
					WarningMessages: []
				}
			];
			var actionTargets = [
				{
					PersonId: 'testPerson1',
					Name: 'testPerson1'
				}
			];
			var commandInfo = {
				error: 'error'
			};
			target.reportActionResult(commandInfo, actionTargets, actionResults);

			expect(fakeNotice.calledCountForWarning).toBe(0);

		});

		it('should notify error when action result contains errors', function () {
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
			expect(fakeNotice.getLastError()).toBe('error happens : testPerson1');

		});

		it('should not list agent names and showing how many agents are effected if more than 20 agents have same error', function () {
			fakeNotice.reset();
			var actionResults = [];
			var actionTargets = [];

			for (var i = 0; i <= 21; i++) {
				actionTargets.push({
					PersonId: 'testPerson' + i,
					Name: 'testPerson' + i
				});
				actionResults.push({
					PersonId: 'testPerson' + i,
					ErrorMessages: ['error happens']
				})
			}

			var commandTemplates = {
				success: 'success',
				warning: 'partial success'
			};

			target.reportActionResult(commandTemplates, actionTargets, actionResults);

			expect(fakeNotice.getLastError()).toBe('error happens : AffectingXAgents');
		})

		function FakeNotice() {
			var warning = '';
			var error = '';
			var success = '';
			this.calledCountForWarning = 0;
			this.warning = function (message, time, destroyOnStateChange) {
				this.calledCountForWarning++;
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
			this.getLastError = function () {
				return error;
			}

		};
	});
})();
