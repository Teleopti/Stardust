'use strict';

var searchService = angular.module('restNotificationService', ['ngResource']);
searchService.service('NotificationSvrc', [
	'$resource', function ($resource) {
		//this.search = $resource('../api/Search/People', { keyword: "@searchKey" }, {
		//	query: { method: 'GET', params: {  }, isArray: true }
		//});
		this.notifications = ['Quick forcast triggered by Ashley', 'Scheduled published for team yellow', 'Scheduling complete for 2015-04-15 to 2015-05-16'];
	}
]);