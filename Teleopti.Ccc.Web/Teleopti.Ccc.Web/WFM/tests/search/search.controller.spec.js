'use strict';
describe('SearchCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
	}));

	var mockSearchService = {
		search: {
			query: function () {
				var queryDeferred = $q.defer();
				var result = [{ Name: '', SearchGroup: 'PlanningPeriod' }, { Name: '', SearchGroup: 'Search' }];
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	
	it('return the correct number of search groups', inject(function ($controller) {
		var scope = $rootScope.$new();

		$controller('SearchCtrl', { $scope: scope, SearchSvrc: mockSearchService });

		scope.searchKeyword('sd');
		scope.$digest();
		
		expect(scope.searchResultGroups.length).toEqual(2);
	}));
});
