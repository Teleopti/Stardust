(function(){
	'use strict';
	angular.module('wfm.personSchedule')
		.controller('PersonScheduleCtrl', ['People', PersonScheduleController]);
	function PersonScheduleController(PeopleSvc) {
		var vm = this;
		PeopleSvc.loadPeopleInMyTeam.get().$promise.then(function(data) {
			vm.People = data;
		});
	}
}());