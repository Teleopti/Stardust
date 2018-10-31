(function () {
	'use strict';

	if (!Array.prototype.find) {
		Array.prototype.find = function (predicate) {
			if (this == null) {
				throw new TypeError('Array.prototype.find called on null or undefined');
			}
			if (!angular.isFunction(predicate)) {
				throw new TypeError('predicate must be a function');
			}
			var list = Object(this);
			var length = list.length >>> 0;
			var thisArg = arguments[1];
			var value;

			for (var i = 0; i < length; i++) {
				value = list[i];
				if (predicate.call(thisArg, value, i, list)) {
					return value;
				}
			}
			return undefined;
		};
	}

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['NoticeService', '$translate', teamScheduleNotificationService]);

	function teamScheduleNotificationService(NoticeService, $translate) {
		var notificationDisplayTime = 5000;

		this.notify = notify;
		this.reportActionResult = reportActionResult;
		this.buildConfirmationMessage = buildConfirmationMessage;

		function reportActionResult(commandInfo, actionTargets, actionResults) {
			var failActionResults = [];
			var warningActionResults = [];
			actionResults.forEach(function (x) {
				if (x.ErrorMessages && x.ErrorMessages.length > 0) {
					failActionResults.push({
						PersonId: x.PersonId,
						Messages: x.ErrorMessages
					});
				}
				if (x.WarningMessages && x.WarningMessages.length > 0) {
					warningActionResults.push({
						PersonId: x.PersonId,
						Messages: x.WarningMessages
					});
				}
			});

			var actionInfo = [actionTargets.length, actionTargets.length - failActionResults.length, failActionResults.length];

			if (failActionResults.length === 0) {
				var successMessage = replaceParams(commandInfo.success, actionInfo);
				NoticeService.success(successMessage, notificationDisplayTime, true);
				noticeWarningMessagesIfHave(warningActionResults, actionTargets);
				return;
			}

			if (!!commandInfo.warning) {
				var warningMessage = replaceParams(commandInfo.warning, actionInfo);
				NoticeService.warning(warningMessage, notificationDisplayTime, true);
			}

			noticeErrorMessagesIfHave(failActionResults, actionTargets);

			noticeWarningMessagesIfHave(warningActionResults, actionTargets);

		}

		function buildConfirmationMessage(text, personCount, activityCount) {
			return replaceParams(text, [activityCount, personCount]);
		}

		function notify(type, text, params) {
			var message = replaceParams(text, params);
			switch (type) {
				case 'success':
					NoticeService.success(message, notificationDisplayTime, true);
					break;

				case 'error':
					NoticeService.error(message, null, true);
					break;

				case 'warning':
					NoticeService.warning(message, notificationDisplayTime, true);
					break;

				default:
					break;
			}
			return message;
		}


		function noticeErrorMessagesIfHave(actionResults, actionTargets) {
			noticeIfHave(actionResults, actionTargets, function (value, key) {
				var agentInfo = value.length > 20 ? $translate.instant('AffectingXAgents').replace('{0}', value.length) : value.join(", ");
				var errorMessage = key + " : " + agentInfo;
				NoticeService.error(errorMessage, null, true);
			});
		}

		function noticeWarningMessagesIfHave(actionResults, actionTargets) {
			noticeIfHave(actionResults, actionTargets, function (value, key) {
				var warning = key + " : " + value.join(", ");
				NoticeService.warning(warning, null, true);
			});
		}

		function noticeIfHave(actionResults, actionTargets, doNotice) {
			if (!actionResults || !actionResults.length) return;
			var results = {};
			angular.forEach(actionResults, function (result) {
				var messages = result.Messages;
				var personName = actionTargets.find(function (t) { return t.PersonId === result.PersonId; }).Name;
				angular.forEach(messages, function (message) {
					collectResult(message, personName, results);
				});
			});

			angular.forEach(results, doNotice);
			return results;
		}

		function collectResult(key, value, results) {
			if (angular.isUndefined(results[key]))
				results[key] = [value];
			else if (results[key].indexOf(value) < 0)
				results[key].push(value);
		};

		function replaceParams(text, params) {
			if (!text) return '';
			params && params.forEach(function (param, index) {
				if (angular.isDefined(param))
					text = text.replace('{' + index + '}', param);
			});
			return text;
		}
	}
})();
