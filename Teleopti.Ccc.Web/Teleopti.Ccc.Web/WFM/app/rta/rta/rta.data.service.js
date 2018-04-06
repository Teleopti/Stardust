(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaDataService', rtaDataService);

	function rtaDataService($state, rtaService, $q, $http, $translate) {

		var organization = [];
		var skills = [];
		var skillAreas = [];
		var states = [];

		var loaded = $q.all([
			rtaService.getOrganization()
				.then(function (data) {
					organization = data;
					organization.forEach(function (site) {
						site.Teams = site.Teams || [];
					});
				}),
			rtaService.getSkills()
				.then(function (data) {
					skills = data;
				}),
			rtaService.getSkillAreas()
				.then(function (data) {
					skillAreas = data;
				}),
			$http.get('../api/PhoneStates')
				.then(function (response) {
					states = response.data;
					states.push({Id: null, Name: $translate.instant('NoState')});
				})
		]).then(function () {
			return {
				organization: organization,
				skills: skills,
				skillAreas: skillAreas,
				states: states
			}
		});

		return {
			load: function () {
				return loaded;
			}
		};

	}

})();