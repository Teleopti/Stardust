(function () {
	'use strict';

angular.module('wfm.people')
	.controller('PeopleStart', [ '$scope', PeopleStartController ]);

	function PeopleStartController($scope) {

		$scope.people = [
			{
				firstName: 'Jhon',
				id: '1',
				lastName: 'Doe',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Jennie',
				id: '2',
				lastName: 'Flowers',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Bert',
				id: '3',
				lastName: 'Erickson',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Carolyn',
				id: '4',
				lastName: 'Garrett',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Christian',
				id: '5',
				lastName: 'Reeves',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Sergio',
				id: '6',
				lastName: 'Evans',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Edwin',
				id: '7',
				lastName: 'Morrison',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Tom',
				id: '8',
				lastName: 'Vaughn',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Ollie',
				id: '9',
				lastName: 'Norris',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Greg',
				id: '10',
				lastName: 'Harrington',
				role: 'Agent',
				phone: '123456123'
			},
			{
				firstName: 'Clyde',
				id: '11',
				lastName: 'Wilkins',
				role: 'Agent',
				phone: '123456123'
			},

		];

		$scope.paginationOptions = { pageNumber: 1, totalPages: 7 };
		$scope.getPageData = function (pageIndex) {
			angular.log(pageIndex);
		};

		$scope.multi = false;
		$scope.itemArr = [];
		$scope.assertMulti = function (item) {
			if ($scope.itemArr.length > 0) {
				$scope.multi = true;
				$scope.itemArr.push(item);
			} else {
				$scope.multi = true;
				$scope.itemArr.push(item);
			}
		}
		$scope.resetMulti = function () {
			$scope.itemArr = [];
			$scope.multi = false;
			for (var i = 0; i < $scope.people.length; i++) {
				$scope.people[i].marked = false;
			}
		}

	}
})();