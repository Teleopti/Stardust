(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.factory('rtaDataService', rtaDataService);

	function rtaDataService($q, $http, $translate) {

		var organization = [];
		var skills = [];
		var skillAreas = [];
		var states = [];
		var permissions = {};

		var loaded = null;

		function load() {
			return $q.all([
				$http.get('../api/Sites/Organization')
					.then(function (response) {
						organization = response.data;
						organization.forEach(function (site) {
							site.Teams = site.Teams || [];
						});
					}),
				$http.get('../api/Skills')
					.then(function (response) {
						skills = response.data;
					}),
				$http.get('../api/SkillGroups')
					.then(function (response) {
						skillAreas = response.data;
					}),
				$http.get('../api/PhoneStates')
					.then(function (response) {
						states = response.data;
						states.push({Id: null, Name: $translate.instant('NoState')});
					}),
				$http.get('../api/Adherence/Permissions')
					.then(function(response){
						permissions  = response.data;
					})
			]).then(function () {
				return {
					organization: organization,
					skills: skills,
					skillAreas: skillAreas,
					states: states,
					permissions: permissions
				}
			})
		}

		return {
			load: function () {
				if (!loaded)
					loaded = load();
				return loaded;
			},
			reload: function () {
				loaded = load();
				return loaded;
			}
		};

	}

})();