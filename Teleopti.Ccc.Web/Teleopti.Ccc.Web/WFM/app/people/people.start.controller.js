(function () {
	'use strict';

	angular.module('wfm.people')

		.controller('PeopleStart', ['$scope', PeopleStartController]);

	function PeopleStartController($scope) {

		$scope.getNumberOfCheckedRoles = function (roles) {
			var checkedRoles = 0;
			angular.forEach(roles, function (value, key) {
				if (value.checked) {
					checkedRoles++;
				}
			});

			return checkedRoles;
		};

		$scope.roles = [
			{ name: 'Agent' },
			{ name: 'Team leader' },
			{ name: 'London, site admin' },
			{ name: 'Partner Web' },
			{ name: 'Store staff' },
			{ name: 'Super Administrator Role' }
		];

		$scope.currentRoles = [];


		$scope.people = [
			{
				firstName: 'John',
				id: '1',
				lastName: 'Doe',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: true },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Jennie',
				id: '2',
				lastName: 'Flowers',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: true }
				]
			},
			{
				firstName: 'Bert',
				id: '3',
				lastName: 'Erickson',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Carolyn',
				id: '4',
				lastName: 'Garrett',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Christian',
				id: '5',
				lastName: 'Reeves',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Sergio',
				id: '6',
				lastName: 'Evans',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Edwin',
				id: '7',
				lastName: 'Morrison',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Tom',
				id: '8',
				lastName: 'Vaughn',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Ollie',
				id: '9',
				lastName: 'Norris',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Greg',
				id: '10',
				lastName: 'Harrington',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
			{
				firstName: 'Clyde',
				id: '11',
				lastName: 'Wilkins',
				role: 'Agent',
				roles: [
					{ name: 'Agent', checked: true },
					{ name: 'Team leader', checked: false },
					{ name: 'London, site admin', checked: false },
					{ name: 'Partner Web', checked: false },
					{ name: 'Store staff', checked: false },
					{ name: 'Super Administrator Role', checked: false }
				]
			},
		];
		$scope.disabledOptions = true;

		$scope.checkExistingRoles = function (role) {
			var exists = false;
			angular.forEach($scope.currentRoles, function (value, key) {
				if (value.name === role.name) {
					exists = true;
				}
			});
			return exists;
		}


		$scope.checkRolesNotOnAll = function (role) {
			var exists = false;
			angular.forEach($scope.currentRoles, function (value, key) {
				if (value.name === role.name && value.usedBy === $scope.itemArr.length) {
					exists = true;
				}
			});
			return exists;
		}

		$scope.paginationOptions = { pageNumber: 1, totalPages: 7 };
		$scope.getPageData = function (pageIndex) {
			angular.log(pageIndex);
		};

		$scope.multi = false;
		$scope.itemArr = [];
		$scope.assertMulti = function (item) {

			$scope.currentRoles.length = 0;
			var indexOfItem = $scope.itemArr.indexOf(item);

			if (indexOfItem !== -1) {
				$scope.itemArr.splice(indexOfItem, 1);
			} else {
				$scope.itemArr.push(item);
			}


			angular.forEach($scope.itemArr, function (person, key) {
				angular.forEach(person.roles, function (role, key) {
					if (role.checked) {
						if ($scope.checkIfRoleExistInCurrentRoles(role) === false) {
							role.usedBy = 1;
							$scope.currentRoles.push(angular.copy(role));
						} else {
							$scope.incrementUsedBy(role);
						}
					}
				});
			});
		};

		$scope.isIndeterminate = function (role) {
			return role.usedBy !== $scope.itemArr.length;
		};

		$scope.isChecked = function (role) {
			return role.usedBy === $scope.itemArr.length;
		};

		$scope.incrementUsedBy = function (role) {
			angular.forEach($scope.currentRoles, function (currentRole, key) {
				if (role.name === currentRole.name) {
					currentRole.usedBy++;
				}
			});
		};

		$scope.checkIfRoleExistInCurrentRoles = function (role) {
			var returnValue = false;
			angular.forEach($scope.currentRoles, function (currentRole, key) {
				if (role.name === currentRole.name) {
					returnValue = true;
				}
			});

			return returnValue;
		};

		$scope.modalShownGrant = false;
		$scope.modalShownRevoke = false;
		$scope.showSelected = true;

		$scope.peopleToShow = $scope.people;

		$scope.showAll = function () {
			if (!$scope.showSelected) {
				$scope.showSelected = true;
				$scope.peopleToShow = $scope.people;
			} else {
				$scope.showSelected = false;
				$scope.peopleToShow = $scope.itemArr;
			}
		}

		$scope.close = function () {
			$scope.modalShownGrant = false;
			$scope.modalShownRevoke = false;
			$scope.newRoles.length = 0;
		}

		$scope.resetMulti = function () {
			$scope.itemArr = [];
			$scope.multi = false;
			for (var i = 0; i < $scope.people.length; i++) {
				$scope.people[i].marked = false;
			}
		}

		$scope.buttonText = function () {
			if ($scope.showSelected) {
				return "Show selected (" + $scope.itemArr.length + ")";
			} else {
				return "Show all";
			}
		}

		$scope.newRoles = [];

		$scope.addRole = function (role, shouldCheck) {
			var indexOfRole = $scope.newRoles.indexOf(role);
			if (role.checked === shouldCheck) {
				if (indexOfRole === -1) {
					$scope.newRoles.push(role);
				}
			} else {
				if (indexOfRole !== -1) {
					$scope.newRoles.splice(indexOfRole, 1);
				}
			}
		}

		$scope.save = function (shouldGrant) {
			angular.forEach($scope.itemArr, function (person, key) {
				angular.forEach(person.roles, function (role, key) {
					angular.forEach($scope.newRoles, function (newRole, key) {
						if(role.name === newRole.name) {
							console.log(shouldGrant);
							role.checked = shouldGrant;
						}
					});
				});
			});

			$scope.newRoles.length = 0;
			$scope.resetMulti();

			$scope.modalShownGrant = false;
			$scope.modalShownRevoke = false;
		}
	}
})();