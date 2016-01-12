﻿'use strict';
describe('RequestsControllerTests', function () {
	var $rootScope,
		$controller,
		requestsToggles,
		requestCommandParamsHolder;

	beforeEach(function () {
		module('wfm.requests');

		requestsToggles = new fakeRequestsToggles();
		requestCommandParamsHolder = new fackRequestCommandParamsHolder();

		module(function ($provide) {
			$provide.service('RequestsToggles', function () {
				return requestsToggles;
			});
			$provide.service('requestCommandParamsHolder', function () {
				return requestCommandParamsHolder;
			});
		});
	});

	beforeEach(inject(function (_$rootScope_, _$controller_) {
		$rootScope = _$rootScope_;
		$controller = _$controller_;
	}));


	it('should show selected requests information when requests get selected and nothing vice verse', function () {
		var test = setUpTarget();

		var requestIds = [{id:1}, {id:2}];
		requestCommandParamsHolder.setSelectedRequestsIds(requestIds);

		test.target.selectedRequestsInfoText = 'Selected {0} of {1} requests';
		test.target.paging.totalRequestsCount = 10;
		test.scope.$digest();

		expect(test.target.showSelectedRequestsInfo()).toEqual('Selected 2 of 10 requests');

		requestCommandParamsHolder.setSelectedRequestsIds([]);
		test.scope.$digest();
		expect(test.target.showSelectedRequestsInfo()).toEqual('');
	});


	function setUpTarget() {
		var scope = $rootScope.$new();
		var target = $controller('RequestsCtrl', {
			$scope: scope
		});
		return { target: target, scope: scope };
	}

	function fackRequestCommandParamsHolder() {
		var requestIds;
		this.setSelectedRequestsIds = function (ids) {
			requestIds = ids;
		}
		this.getSelectedRequestsIds = function () {
			return requestIds;
		}
	}

	function fakeRequestsToggles() {

		this.togglePromise = function() {
			var queryDeferred = $q.defer();
			queryDeferred.resolve({});
			return { $promise: queryDeferred.promise };
		}
		var toggles = {
			isRequestsEnabled: function() {
				return true;
			},
			isPeopleSearchEnabled: function() {
				return true;
			},
			isPaginationEnabled: function() {
				return true;
			},
			isRequestsCommandsEnabled: function() {
				return true;
			}
		}
		this.togglePromise.then = function(cb) {
			if (cb) cb(toggles);
		}

	}
});
