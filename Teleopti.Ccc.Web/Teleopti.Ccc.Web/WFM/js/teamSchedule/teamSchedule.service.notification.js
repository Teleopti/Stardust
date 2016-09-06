(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['NoticeService', '$translate', teamScheduleNotificationService]);

	function teamScheduleNotificationService(NoticeService, $translate) {
		this.notify = notify;

		this.reportActionResult = reportActionResult;
		this.buildConfirmationMessage = buildConfirmationMessage;

		function reportActionResult(commandInfo, actionTargets, actionResults) {
			var failActionResults = [];
			var warningActionResults = [];
			actionResults.forEach(function(x) {
				if (x.ErrorMessages.length > 0) {
					failActionResults.push({
						PersonId: x.PersonId,
						Messages: x.ErrorMessages
					});
				}
				if (x.WarningMessages.length > 0) {
					warningActionResults.push({
						PersonId: x.PersonId,
						Warnings: x.WarningMessages
					});
				}
			});
			var actionInfo = [actionTargets.length, actionTargets.length - failActionResults.length, failActionResults.length];
			var successMessage = replaceParams($translate.instant(commandInfo.success), actionInfo);
			var warningMessage = replaceParams($translate.instant(commandInfo.warning), actionInfo);
			
			var warningResults = {};
			if (failActionResults.length === 0 && warningActionResults.length === 0) {
				NoticeService.success(successMessage, 5000, true);
				return;
			}
			else if (failActionResults.length === 0 && warningActionResults.length > 0) {
				NoticeService.success(successMessage, 5000, true);

				angular.forEach(warningActionResults, function (result) {
					var messages = result.Warnings;
					var personName = actionTargets.find(function (t) { return t.PersonId === result.PersonId; }).Name;
					angular.forEach(messages, function (message) {
						collectResult(message, personName, warningResults);
					});
				});
				angular.forEach(warningResults, function (value, key) {
					var warning = key + " : " + value.join(", ");
					NoticeService.warning(warning, null, true);
				});
				return;
			}
			
			NoticeService.warning(warningMessage, 5000, true);

			var errorResults = {}

			angular.forEach(failActionResults, function(result) {
				var messages = result.Messages;
				var personName = actionTargets.find(function(t) { return t.PersonId === result.PersonId; }).Name;
				angular.forEach(messages, function(message) {
					collectResult(message, personName, errorResults);
				});
			});

			angular.forEach(errorResults, function (value, key) {
				var errorMessage = key + " : " + value.join(", ");
				NoticeService.error(errorMessage, null, true);
			});
			angular.forEach(warningActionResults, function (result) {
				var messages = result.Warnings;
				var personName = actionTargets.find(function (t) { return t.PersonId === result.PersonId; }).Name;
				angular.forEach(messages, function (message) {
					collectResult(message, personName, warningResults);
				});
			});
			angular.forEach(warningResults, function (value, key) {
				var warning = key + " : " + value.join(", ");
				NoticeService.warning(warning, null, true);
			});
			
		}

		function buildConfirmationMessage(template, personCount, activityCount, needTranslate) {
			var text = needTranslate ? $translate.instant(template) : template;

			return activityCount != null ? replaceParams(text, [activityCount, personCount]) : replaceParams(text, [personCount]);
		}

		function notify(type, template, params) {
			var translatedTemplate = $translate.instant(template);
			var message = replaceParams(translatedTemplate, params);
			switch (type) {
				case 'success':
					NoticeService.success(message, 5000, true);
					break;

				case 'error':
					NoticeService.error(message, null, true);
					break;

				case 'warning':
					NoticeService.warning(message, 5000, true);
					break;

				default:
					break;
			}
			return message;
		}

		function collectResult(key, value, results) {
			if (results[key] === undefined)
				results[key] = [value];
			else if (results[key].indexOf(value) < 0)
				results[key].push(value);
		};

		function replaceParams(text, params) {
			if (params) {
				params.forEach(function (element, index) {
					text = text.replace('{' + index + '}', element);
				});
			}
			return text;
		}
	}
})();
