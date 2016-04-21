(function() {
	'use strict';

	angular.module('wfm.teamSchedule')
		.service('teamScheduleNotificationService', ['NoticeService', '$translate', teamScheduleNotificationService]);

	function teamScheduleNotificationService(NoticeService, $translate) {
		this.notify = notify;

		function notify(type, template, params) {
			var translatedTemplate = $translate.instant(template);
			var message = replaceParams(translatedTemplate, params);
			switch (type) {
				case 'success':
					NoticeService.success(message, 5000, true);
					break;

				case 'error':
					NoticeService.error(message, 5000, true);
					break;

				case 'warning':
					NoticeService.warning(message, 5000, true);
					break;

				default:
					break;
			}
			return message;
		}

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
