(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaConfigurationValidator', rtaConfigurationValidator);

	function rtaConfigurationValidator($http, $translate, NoticeService) {
		return {
			validate: function () {
				return $http.get('../Rta/Configuration/Validate/')
					.then(function (response) {
						if (!response.data.length)
							return;

						var header = $translate.instant('RtaConfigurationIssuesFound') + '</br></br><ul>';
						var notification = response.data.reduce(function (notification, message) {
							notification += '<li>' + $translate.instant(message.Resource) + '</li>';
							(message.Data || []).forEach(function (data, index) {
								notification = notification.replace('{' + index + '}', data);
							});
							return notification;
						}, header);

						NoticeService.warning(notification, 10000, true);
					});
			}
		};
	}

})();