(function () {
	'use strict';

	angular
		.module('wfm.rta')
		.controller('RtaMainController', RtaMainController);

	RtaMainController.$inject = ['rtaService', 'rtaRouteService', 'skills', 'skillAreas', '$state', '$stateParams', '$interval', '$scope', '$q', '$timeout'];

	function RtaMainController(rtaService, rtaRouteService, skills, skillAreas, $state, $stateParams, $interval, $scope, $q, $timeout) {
		var vm = this;
		vm.skillIds = $stateParams.skillIds || [];
		vm.skillAreaId = $stateParams.skillAreaId;
		$stateParams.open = ($stateParams.open || "false");
		vm.skills = skills || [];
		vm.skillAreas = skillAreas || [];
		vm.siteCards = [];
		vm.totalAgentsInAlarm = 0;
		vm.urlParams = $stateParams;
		vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
		vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id})';
		vm.organizationSelection = false;
		vm.skillSelected = vm.skillIds.length;
		vm.goToAgentsView = function () { rtaRouteService.goToSelectSkill(); };
		vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };

		var pollPromise;

		(function () {
			if (angular.isDefined(vm.urlParams.skillAreaId)) {
				vm.selectedItems.skillAreaId = vm.urlParams.skillAreaId;
				vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillAreaId: "' + vm.urlParams.skillAreaId + '"})';
				vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id, skillAreaId: "' + vm.urlParams.skillAreaId + '"})';
			}
			else if (angular.isDefined(vm.urlParams.skillIds)) {
				vm.selectedItems.skillIds = vm.urlParams.skillIds;
				vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillIds: ["' + vm.urlParams.skillIds[0] + '"]})';
				vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id, skillIds: ["' + vm.urlParams.skillIds[0] + '"]})';
			}
			else {
				vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };
				vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
				vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id})';
			}

			if (angular.isDefined(vm.skillAreaId)) {
				vm.skillIds = getSkillIdsFromSkillAreaId(vm.skillAreaId);
			}



		})();

		function pollInitiate() {
			vm.siteCards = [];
			pollNow();
		}

		function pollNow() {
			pollStop();
			getSites()
				.then(getTeamsForSites)
				.then(pollNext);
		}

		function pollNext() {
			pollPromise = $timeout(getData, 5000);
		}

		function pollStop() {
			if (pollPromise)
				$timeout.cancel(pollPromise);
		}

		pollInitiate();
		
		$scope.$on('$destroy', pollStop);

		function getData() {
			$q.all([
				getSites(),
				getTeamsForSites()
			]).then(pollNext);
		};

		function getSites() {
			return rtaService.getOverviewModelFor(vm.skillIds)
				.then(function (sites) {
					sites.Sites.forEach(function (site) {
						var siteCard = vm.siteCards.find(function (siteCard) {
							return siteCard.site.Id === site.Id;
						});
						if (!siteCard) {
							siteCard = {
								site: site,
								isOpen: $stateParams.open != "false"
							};
							$scope.$watch(function () { return siteCard.isOpen }, pollNow);
							vm.siteCards.push(siteCard);
						};
						siteCard.site.Color = translateSiteColors(site);
						siteCard.site.InAlarmCount = site.InAlarmCount;
					});
					vm.totalAgentsInAlarm = sites.TotalAgentsInAlarm;
					vm.noSiteCards = !vm.siteCards.length;
				});
		}

		function getTeamsForSites() {
			return $q.all(
				vm.siteCards
					.filter(function (s) { return s.isOpen; })
					.map(function (s) {
						return getTeamsForSite(s);
					})
			);
		}

		function getTeamsForSite(s) {
			return rtaService.getTeamCardsFor({ siteIds: s.site.Id, skillIds: vm.skillIds })
				.then(function (teams) {
					s.teams = teams;
					setTeamToSelected(s);
				});
		}

		function setTeamToSelected(card) {
			var indexOfSite = vm.selectedItems.siteIds.indexOf(card.site.Id);
			if (indexOfSite > -1) {
				card.teams.forEach(function (team) {
					team.isSelected = true;
				});
			}
			else {
				card.teams.forEach(function (team) {
					var indexOfTeam = vm.selectedItems.teamIds.indexOf(team.Id);
					if (indexOfTeam > -1) team.isSelected = true;
				});
			}
		}

		function getSkillIdsFromSkillAreaId(id) {
			return vm.skillAreas.find(function (sa) { return sa.Id === id; })
				.Skills.map(function (skill) { return skill.Id; });
		}

		function translateSiteColors(site) {
			if (site.Color === 'good') {
				return '#C2E085';
			} else if (site.Color === 'warning') {
				return '#FFC285';
			} else if (site.Color === 'danger') {
				return '#EE8F7D';
			} else {
				return '#fff';
			}
		}

		vm.filterOutput = function (selectedItem) {
			if (!angular.isDefined(selectedItem)) {
				resetOnNoSkills();
			} else if (selectedItem.hasOwnProperty('Skills')) {
				setUpForSkillArea(selectedItem);
			} else {
				setUpForSkill(selectedItem);
			}
			pollInitiate();
		}

		function resetOnNoSkills() {
			vm.skillIds = [];
			vm.urlParams.skillIds = undefined;
			vm.urlParams.skillAreaId = undefined;
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: undefined };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id})';
			vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: undefined }, { notify: false });
		}

		function setUpForSkillArea(selectedItem) {
			vm.skillIds = getSkillIdsFromSkillAreaId(selectedItem.Id);
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: [], skillAreaId: selectedItem.Id };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillAreaId: "' + selectedItem.Id + '"})';
			vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id, skillAreaId: "' + selectedItem.Id + '"})';
			$state.go($state.current.name, { skillAreaId: selectedItem.Id, skillIds: undefined }, { notify: false });
		}

		function setUpForSkill(selectedItem) {
			vm.skillIds = [selectedItem.Id];
			vm.selectedItems = { siteIds: [], teamIds: [], skillIds: vm.skillIds, skillAreaId: undefined };
			vm.agentsState = 'rta.agents({siteIds: card.site.Id, skillIds: ["' + selectedItem.Id + '"]})';
			vm.agentsStateForTeam = 'rta.agents({teamIds: team.Id, skillIds: ["' + selectedItem.Id + '"]})';
			$state.go($state.current.name, { skillAreaId: undefined, skillIds: vm.skillIds }, { notify: false });
		}


		vm.getSelectedItems = function (siteCard) {
			var selectedItemsHandler = createSelectedItemsHandler(vm.selectedItems);

			if (angular.isDefined(siteCard.site)) {
				selectSite(selectedItemsHandler, siteCard);
			} else {
				selectTeam(selectedItemsHandler, siteCard);
			}

			vm.organizationSelection = vm.selectedItems.siteIds.length || vm.selectedItems.teamIds.length;
		}

		function selectSite(selectedItemsHandler, site) {
			var siteAlreadySelected = vm.selectedItems.siteIds.indexOf(site.site.Id) > -1;

			if (siteAlreadySelected) {
				selectedItemsHandler.removeSite(site);
			} else {
				selectedItemsHandler.addSite(site);

				if (angular.isDefined(site.teams)) {
					selectedItemsHandler.clearTeams(site);
				}
			}
		}

		function selectTeam(selectedItemsHandler, team) {
			var parentSite = vm.siteCards.find(function (card) { return card.site.Id === team.SiteId; });
			var siteNoLongerHasAllTeamsSelected = !parentSite.isSelected && vm.selectedItems.siteIds.indexOf(parentSite.site.Id) > -1;
			var allTeamsInSiteAreNowSelected = parentSite.isSelected;

			if (allTeamsInSiteAreNowSelected) {
				selectedItemsHandler
					.clearTeams(parentSite)
					.addSite(parentSite);

			} else if (siteNoLongerHasAllTeamsSelected) {
				selectedItemsHandler
					.removeSite(parentSite)
					.addAllTeams(parentSite)
					.toggleTeam(team);

			} else {
				selectedItemsHandler
					.toggleTeam(team);
			}
		}

		function createSelectedItemsHandler(selecteItems) {
			var that = {
				addSite: addSite,
				removeSite: removeSite,
				addAllTeams: addAllTeams,
				clearTeams: clearTeams,
				toggleTeam: toggleTeam
			};

			var _selectedItems = selecteItems;

			function addSite(parentSite) {
				_selectedItems.siteIds.push(parentSite.site.Id);
				return that;
			}

			function removeSite(parentSite) {
				var selectedSiteIndex = _selectedItems.siteIds.indexOf(parentSite.site.Id);
				_selectedItems.siteIds.splice(selectedSiteIndex, 1);
				return that;
			}

			function addAllTeams(parentSite) {
				parentSite.teams.forEach(function (team) {
					_selectedItems.teamIds.push(team.Id);
				});
				return that;
			}

			function clearTeams(parentSite) {
				parentSite.teams.forEach(function (team) {
					var index = _selectedItems.teamIds.indexOf(team.Id);
					_selectedItems.teamIds.splice(index, 1);
				});
				return that;
			}

			function toggleTeam(item) {
				var indexOfTeam = _selectedItems.teamIds.indexOf(item.Id);
				var teamNotSelected = indexOfTeam == -1;
				if (teamNotSelected)
					_selectedItems.teamIds.push(item.Id);
				else
					_selectedItems.teamIds.splice(indexOfTeam, 1);
				return that;
			}

			return that;
		}

		vm.goToAgents = function () {
			$state.go('rta.agents', vm.selectedItems);
		}
	}
})();
