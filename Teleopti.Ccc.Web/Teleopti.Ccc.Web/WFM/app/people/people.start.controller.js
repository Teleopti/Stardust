const fetchDefaultOptions = {
	method: 'GET',
	headers: {
		Accept: 'application/json',
		'Content-Type': 'application/json',
		Cache: 'no-cache'
	},
	credentials: 'include'
};

// Fetch wrapper for json
const fetchJSON = async (input, init) => {
	return fetch(input, init).then(response => response.json());
};

// Array.map
const personToId = ({ PersonId }) => PersonId;

// Array.sort
const sortRolesByName = (role1, role2) => role1.Name >= role2.Name;
const sortPeopleByName = (person1, person2) => person1.FirstName >= person2.FirstName;

// API calls
const searchPeople = async (keyword = 'a') => {
	return fetchJSON(
		`../api/Search/People/Keyword?currentPageIndex=1&keyword=${keyword}&pageSize=10&sortColumns=LastName:true`,
		fetchDefaultOptions
	).then(({ People }) => People);
};

const getRoles = async () => {
	return fetchJSON('../api/PeopleData/fetchRoles', fetchDefaultOptions);
};

const getPersons = async ({ Date = '2017-02-08', PersonIdList }) => {
	return fetchJSON(`../api/PeopleData/fetchPersons`, {
		...fetchDefaultOptions,
		method: 'POST',
		body: JSON.stringify({ Date, PersonIdList })
	});
};

const getPeople = async () => {
	const peopleResult = await searchPeople();
	const PersonIdList = peopleResult.map(personToId);

	const persons = await getPersons({ PersonIdList });
	return persons
		.map(person => ({
			...person,
			selected: false
		}))
		.sort(sortPeopleByName);
};

(function() {
	'use strict';
	angular.module('wfm.people').controller('PeopleStart', ['$scope', PeopleStartController]);

	function PeopleStartController($scope) {
		$scope.roles = [];
		$scope.people = [];
		$scope.selectedPeople = [];
		$scope.selectedPeopleIds = [];
		$scope.selectedPeopleRoleIds = [];
		$scope.selectedRoleIds = [];

		getRoles().then(roles => {
			$scope.roles = roles;
			$scope.$digest();
		});

		getPeople().then(people => {
			$scope.people = people;
			$scope.$digest();
		});

		$scope.toggleSelectedPerson = person => {
			if ($scope.selectedPeopleIds.includes(person.Id))
				$scope.selectedPeopleIds = $scope.selectedPeopleIds.filter(id => id !== person.Id);
			else $scope.selectedPeopleIds = [...$scope.selectedPeopleIds, person.Id];

			console.log($scope.selectedPeopleIds);
		};

		$scope.isSelectedPerson = person => $scope.selectedPeopleIds.includes(person.Id);

		$scope.toggleSelectedRole = roleId => {
			if ($scope.selectedRoleIds.includes(roleId))
				$scope.selectedRoleIds = $scope.selectedRoleIds.filter(id => id !== roleId);
			else $scope.selectedRoleIds = [...$scope.selectedRoleIds, roleId];
		};

		$scope.isSelectedRole = roleId => $scope.selectedRoleIds.includes(roleId);

		$scope.getPerson = id => {
			return $scope.people.find(person => person.Id === id);
		};

		$scope.getRole = id => {
			return $scope.roles.find(role => role.Id === id);
		};

		$scope.$watch('selectedPeopleIds', function() {
			$scope.selectedPeople = $scope.people.filter(person => $scope.selectedPeopleIds.includes(person.Id));

			const roleIds = {};
			$scope.selectedPeople.forEach(person => {
				person.Roles.forEach(role => (roleIds[role.Id] = null));
			});
			$scope.selectedPeopleRoleIds = Object.keys(roleIds);
		});

		$scope.logRoles = roles => {
			console.log(roles);
		};

		$scope.getNumberOfCheckedRoles = roles => {
			return roles.filter(role => role.checked === true).length;
		};

		$scope.joinRoleNames = ({ Roles }) => Roles.map(r => r.Name).join(', ');

		$scope.getRoleMatchesOnSelected = roleId => {
			let count = 0;
			$scope.selectedPeopleIds.forEach(id =>
				$scope.getPerson(id).Roles.forEach(r => {
					if (roleId === r.Id) count++;
				})
			);
			return count;
		};

		$scope.isRoleOnAllSelected = roleId => {
			return $scope.getRoleMatchesOnSelected(roleId) === $scope.selectedPeopleIds.length;
		};

		$scope.paginationOptions = { pageNumber: 1, totalPages: 7 };
		$scope.getPageData = pageIndex => {
			console.log('PageIndex:', pageIndex);
		};

		$scope.selectedPersons = [];
		$scope.assertMulti = function(item) {
			$scope.currentRoles.length = 0;
			var indexOfItem = $scope.selectedPersons.indexOf(item);

			if (indexOfItem !== -1) {
				$scope.selectedPersons.splice(indexOfItem, 1);
			} else {
				$scope.selectedPersons.push(item);
			}

			angular.forEach($scope.selectedPersons, function(person, key) {
				angular.forEach(person.Roles, function(role, key) {
					if (role.checked === false) return;
					if ($scope.checkIfRoleExistInCurrentRoles(role) === false) {
						role.usedBy = 1;
						$scope.currentRoles.push(angular.copy(role));
					} else {
						$scope.incrementUsedBy(role);
					}
				});
			});
		};

		$scope.isUsedBySome = function(roleId) {
			const count = $scope.getRoleMatchesOnSelected(roleId);
			return count > 0;
		};

		$scope.incrementUsedBy = function(role) {
			angular.forEach($scope.currentRoles, function(currentRole, key) {
				if (role.Name === currentRole.Name) {
					currentRole.usedBy++;
				}
			});
		};

		$scope.checkIfRoleExistInCurrentRoles = function(role) {
			var returnValue = false;
			angular.forEach($scope.currentRoles, function(currentRole, key) {
				if (role.Name === currentRole.Name) {
					returnValue = true;
				}
			});

			return returnValue;
		};

		$scope.viewState = {
			showGrant: false,
			showRevoke: false,
			showEditRoles: false,
			showGrantChips: false,
			showRevokeChips: false,
			reset() {
				$scope.viewState.showGrant = false;
				$scope.viewState.showRevoke = false;
				$scope.viewState.showEditRoles = false;
				$scope.viewState.showGrantChips = false;
				$scope.viewState.showRevokeChips = false;
			}
		};

		$scope.close = function() {
			$scope.viewState.reset();
			// $scope.clearSelectedRoles();
		};

		$scope.clearSelectedPeople = () => ($scope.selectedPeopleIds.length = 0);
		$scope.clearSelectedRoles = () => ($scope.selectedRoleIds.length = 0);

		$scope.getInitialState = role => {
			if ($scope.isRoleOnAllSelected(role.Id)) {
				return 'checked';
			} else if ($scope.isUsedBySome(role.Id)) {
				return 'intermediate';
			} else {
				return 'neutral';
			}
		};
		$scope.getRoleState = role => {
			if (typeof role.state === 'undefined') {
				role.state = $scope.getInitialState(role);
			}
			return role.state;
		};

		$scope.roleIsChanged = role => {
			const currentState = $scope.getRoleState(role);
			const initialState = $scope.getInitialState(role);
			const initialChecked = initialState === 'checked';
			const currentChecked = currentState === 'checked';
			console.log('roleIsChanged on', role.Name, currentState, initialState);
			return initialChecked !== currentChecked;
		};

		$scope.toggleRoleState = role => {
			console.log(`Toggle ${role.Name}(${role.Id}) ${role.state}`);
			switch ($scope.getRoleState(role)) {
				case 'neutral':
					if ($scope.isRoleOnAllSelected(role.Id)) {
						role.state = 'checked';
						role.checked = true;
					} else if ($scope.isUsedBySome(role.Id)) {
						role.state = 'intermediate';
						role.checked = false;
					} else {
						role.state = 'checked';
						role.checked = true;
					}
					//role.state = 'checked';
					//role.checked = true;
					break;
				case 'checked':
					role.state = 'neutral';
					role.checked = false;
					break;
				case 'intermediate':
					role.state = 'checked';
					role.checked = true;
					break;
				default:
					role.state = 'neutral';
					role.checked = false;
			}
		};

		$scope.save = function(shouldGrant) {
			// angular.forEach($scope.selectedPersons, function(person, key) {
			// 	angular.forEach(person.Roles, function(role, key) {
			// 		angular.forEach($scope.newRoles, function(newRole, key) {
			// 			if (role.name === newRole.name) {
			// 				console.log(shouldGrant);
			// 				role.checked = shouldGrant;
			// 			}
			// 		});
			// 	});
			// });

			// $scope.newRoles.length = 0;
			// $scope.clearSelectedPeople();
			// $scope.clearSelectedRoles();
			$scope.viewState.reset();
		};
	}
})();
