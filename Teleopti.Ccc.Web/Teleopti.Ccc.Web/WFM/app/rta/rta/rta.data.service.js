(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.service('rtaDataService', rtaDataService);

	function rtaDataService($state, rtaService, $q) {

		var organization = [];
		var skills = [];
		var skillAreas = [];

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
				})
		]).then(function () {
			return {
				organization: organization,
				skills: skills,
				skillAreas
			}
		});

		return {
			load: function () {
				return loaded;
			}
		};

	}

})();