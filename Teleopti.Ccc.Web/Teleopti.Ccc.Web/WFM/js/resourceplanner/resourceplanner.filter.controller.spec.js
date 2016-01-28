/// <reference path="planningperiods.controller.spec.js" />
'use strict';
describe('ResourcePlannerCtrl', function () {
	var $q,
		$rootScope,
		$httpBackend,
		maxHits = 5; //same as in controller

	var filterUrl = function (searchString) {
		return "../api/filters?maxHits=" + maxHits + "&searchString=" & searchString
	}
	var singleFilter = function (id) {
		return "../api/resourceplanner/dayoffrules/"+id
	}
	beforeEach(function() {
		module(function($provide) {
			$provide.service('$stateParams', function() {
				return stateParams;
			});
		});
	});
	beforeEach(module('wfm.resourceplanner'));
	var mockStateParams = {filterId:1,period:1};

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;

		$httpBackend.when('GET', '../api/resourceplanner/dayoffrules/default',
				function (getData) {
					sentData = JSON.parse(getData);
					return true;
				}).respond(200, true);

	}));


	it('should cancel previous search request', inject(function (ResourcePlannerFilterSrvc) {
		var scope = $rootScope.$new();

		var searchString = 'searchString';

		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, []);
		ResourcePlannerFilterSrvc.getData({ searchString: searchString, maxHits: maxHits });
		//should cancel query above - skipping params so not create new one
		ResourcePlannerFilterSrvc.getData();


		scope.$digest();

		$httpBackend.verifyNoOutstandingRequest();
	}));


	it('should load one search result', inject(function ($controller,$stateParams) {
		var searchString = 'something';
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		var scope = $rootScope.$new();
		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, [singleResult]);
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.searchString = searchString;
		$httpBackend.flush();

		var result = scope.results[0];
		expect(result.Id).toEqual(singleResult.Id);
		expect(result.FilterType).toEqual(singleResult.FilterType);
		expect(result.Name).toEqual(singleResult.Name);
	}));

	it('should not put loaded item to selected array', inject(function ($controller,$stateParams) {
		var searchString = 'something';
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, [singleResult]);
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams  });

		scope.searchString = searchString;
		$httpBackend.flush();

		expect(scope.selectedResults.length).toEqual(0);
	}));

	it('should have isSearching set to false before first call', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.isSearching).toEqual(false);
	}));

	it('should set isSearching during search', inject(function ($controller,$stateParams) {
		var searchString = 'something';
		var expectedToHaveBeenTrue=false;

		$httpBackend.whenGET(filterUrl(searchString))
			.respond(200, []);

		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams  });

		scope.$watch(function () { return scope.isSearching; }, function (value) {
			if (value)
				expectedToHaveBeenTrue = true;
		});

		scope.searchString = searchString;


		$httpBackend.flush();

		expect(expectedToHaveBeenTrue).toEqual(true);
		expect(scope.isSearching).toEqual(false);
	}));

	it('should falsify isSearching if non successful call', inject(function ($controller,$stateParams) {
		var searchString = "blabla";

		$httpBackend.whenGET(filterUrl(searchString))
			.respond(500, [{}]);


		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams  });

		scope.searchString = searchString;

		$httpBackend.flush();

		expect(scope.isSearching).toEqual(false);
	}));

	it('should not call service when model is undefined ', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.results).toEqual([]);
	}));

	it('should not call service when model is empty string', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.searchString = '';

		expect(scope.results).toEqual([]);
	}));

	it('should work to call selectedItems before loaded', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.selectedResults.length).toEqual(0);
	}));

	it('should put clicked item in selected', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		var singleResult = {
			Id: 'aölsdf',
			Name: 'asdfasdf',
			FilterType: 'asdfasdfasdf'
		};
		scope.results = [singleResult];

		scope.selectResultItem(singleResult);

		expect(scope.selectedResults[0]).toEqual(singleResult);
	}));

	it('should show that more results exists', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		for (var i = 0; i < maxHits; i++) {
			scope.results.push({ Id: i });
		}

		expect(scope.moreResultsExists()).toEqual(true);
	}));

	it('should not show that more results exists', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		for (var i = 0; i < maxHits - 1; i++) {
			scope.results.push({ Id: i });
		}

		expect(scope.moreResultsExists()).toEqual(false);
	}));
	it('should clear result when no input', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		var loadedResult = {};
		scope.results = [loadedResult];
		scope.searchString = "";
		scope.$digest();

		expect(scope.results.length).toEqual(0);
	}));
	it('should not clear selected when no input', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		var loadedResult = {Name:'hello',Id:'3'};
		scope.results = [loadedResult];
		scope.selectResultItem(loadedResult);
		scope.searchString = "";
		scope.$digest();

		expect(scope.selectedResults.length).toEqual(1);
	}));
	it('should have a name', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.name = "";
		scope.name = "default name";

		expect(scope.name).toBe("default name");

	}));
	it('should set is invalid if no name', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.name = "";

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidName()).toBe(false);
	}));

	it('should set default value for days of per week', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.isValidDayOffsPerWeek()).toBe(true);
		expect(scope.dayOffsPerWeek.MinDayOffsPerWeek).toBe(1); //?
		expect(scope.dayOffsPerWeek.MaxDayOffsPerWeek).toBe(3); //?
	}));


	it('should set invalid if MaxDayOffsPerWeek is smaller than MinDayOffsPerWeek', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 5,
			MaxDayOffsPerWeek: 4
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidDayOffsPerWeek()).toBe(false);
	}));

	it('should set valid if MaxDayOffsPerWeek is equal or greater than MinDayOffsPerWeek', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 4,
			MaxDayOffsPerWeek: 4
		};
		expect(scope.isValidDayOffsPerWeek()).toBe(true);
		scope.dayOffsPerWeek = {
			MinDayOffsPerWeek: 3,
			MaxDayOffsPerWeek: 4
		};
		expect(scope.isValidDayOffsPerWeek()).toBe(true);
	}));

	it('should set default value for consecutive days off', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.isValidConsecDaysOff()).toBe(true);
		expect(scope.consecDaysOff.MinConsecDaysOff).toBe(1); //?
		expect(scope.consecDaysOff.MaxConsecDaysOff).toBe(3); //?
	}));

	it('should set invalid if MaxConsecDaysOff is smaller than MinConsecDaysOff', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.consecDaysOff = {
			MinConsecDaysOff: 5,
			MaxConsecDaysOff: 4
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidConsecDaysOff()).toBe(false);
	}));

	it('should set valid if MaxConsecDaysOff is equal or greater than MinConsecDaysOff', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.consecDaysOff = {
			MinConsecDaysOff: 2,
			MaxConsecDaysOff: 2
		};
		expect(scope.isValidConsecDaysOff()).toBe(true);
		scope.consecDaysOff = {
			MinConsecDaysOff: 1,
			MaxConsecDaysOff: 3
		};
		expect(scope.isValidConsecDaysOff()).toBe(true);
	}));

	it('should set default value for consecutive work days', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		expect(scope.isValidConsecWorkDays()).toBe(true);
		expect(scope.consecWorkDays.MinConsecWorkDays).toBe(2); //?
		expect(scope.consecWorkDays.MaxConsecWorkDays).toBe(6); //?
	}));

	it('should set invalid if MaxConsecWorkDays is smaller than MinConsecWorkDays', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.consecWorkDays = {
			MinConsecWorkDays: 2,
			MaxConsecWorkDays: 1
		};

		expect(scope.isValid()).toBe(false);
		expect(scope.isValidConsecWorkDays()).toBe(false);
	}));

	it('should set valid if MaxConsecWorkDays is equal or greater than MinConsecWorkDays', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.consecWorkDays = {
			MinConsecWorkDays: 4,
			MaxConsecWorkDays: 4
		};
		expect(scope.isValidConsecWorkDays()).toBe(true);
		scope.consecWorkDays = {
			MinConsecWorkDays: 2,
			MaxConsecWorkDays: 5
		};
		expect(scope.isValidConsecWorkDays()).toBe(true);
	}));

	it('should be invalid if no selected result', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });


		expect(scope.isValid()).toBe(false);
		expect(scope.isValidFilters()).toBe(false);
	}));


	it('should be valid if at least one selected result', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.selectedResults = [{}];

		expect(scope.isValidFilters()).toBe(true);
	}));

	it('should be able to save the dayoffrule', inject(function ($controller, $state,$stateParams) {
		var scope = $rootScope.$new();
		var sentData;
		spyOn($state, 'go');


		$controller('ResourceplannerFilterCtrl', { $scope: scope,$state:$state, $stateParams:mockStateParams });
		scope.name = 'asdfasdf';
		scope.filterId = 10;
		scope.default = 'false';

		scope.selectedResults = [{ FilterType: "contract", Id: "5BC5B983-281A-4B92-92D6-A54F00D63A78" }];

		$httpBackend.when('POST', '../api/resourceplanner/dayoffrules',
				function (postData) {
					sentData = JSON.parse(postData);
					return true;
				}).respond(200, true);

		scope.persist();
		$httpBackend.flush();
		expect(sentData.Name).toEqual(scope.name);
		expect(sentData.Id).toEqual(scope.filterId);
		expect(sentData.Default).toEqual(scope.default);
		expect(sentData.MinDayOffsPerWeek).toEqual(scope.dayOffsPerWeek.MinDayOffsPerWeek);
		expect(sentData.MaxDayOffsPerWeek).toEqual(scope.dayOffsPerWeek.MaxDayOffsPerWeek);
		expect(sentData.MinConsecutiveWorkdays).toEqual(scope.consecWorkDays.MinConsecWorkDays);
		expect(sentData.MaxConsecutiveWorkdays).toEqual(scope.consecWorkDays.MaxConsecWorkDays);
		expect(sentData.MinConsecutiveDayOffs).toEqual(scope.consecDaysOff.MinConsecDaysOff);
		expect(sentData.MaxConsecutiveDayOffs).toEqual(scope.consecDaysOff.MaxConsecDaysOff);
		expect(sentData.Filters[0].FilterType).toEqual(scope.selectedResults[0].FilterType);
		expect(sentData.Filters[0].Id).toEqual(scope.selectedResults[0].Id);
		expect($state.go).toHaveBeenCalledWith('resourceplanner.planningperiod',{id:$stateParams.period});

	}));

	it('should not persist when invalid', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.persist();

		scope.$digest();

		$httpBackend.verifyNoOutstandingRequest();
	}));

	it('should not be possible to select item twice', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		var item = {Id:'someGuid'};

		scope.selectResultItem(item);
		scope.selectResultItem(item);

		expect(scope.selectedResults.length).toEqual(1);
	}));

	it('should clear search string when selected', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.searchString = 'something';
		scope.selectResultItem({});

		expect(scope.searchString).toEqual('');
	}));

	it('should clear results when item selected', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });

		scope.results = [{Id:1}];
		scope.selectResultItem({Id:1});

		expect(scope.results.length).toEqual(0);
	}));
	it('should clear the inputfields when clearInput is called', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.results = [{}];
		scope.searchString = 'bla bla';
		scope.clearInput();

		expect(scope.results.length).toEqual(0);
		expect(scope.searchString).toEqual('');
	}));
	it('should remove a selected node when removeNode is called', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.selectedResults = [{id:1},{id:2}]
		scope.removeNode(scope.selectedResults[1]);

		expect(scope.selectedResults.length).toEqual(1);
		expect(scope.selectedResults[0].id).toBe(1)
	}));
	it('should disable button when save is pressed', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.isEnabled = false
		scope.persist();
		expect(scope.isEnabled).toBe(false);
	}));
	it('should not call service if no data is transfered from filter-list', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		$stateParams = {};
		scope.name = "";
		scope.$digest();

		expect(scope.name).toBe("");
	}));
	it('should set new values for options if data is transfered from filter-list', inject(function ($controller) {
		var scope = $rootScope.$new();
		var $stateParams = {filterId:'1',period: '10'};
		var singleResult = {
			Id: '111-111-111',
			Name: 'newName',
		};


		$httpBackend.whenGET(singleFilter($stateParams.filterId))
			.respond(200, singleResult);
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:$stateParams });

		$httpBackend.flush();
		expect(scope.name).toBe("newName");
	}));
	it('should set same id if one already exists', inject(function ($controller) {
		var scope = $rootScope.$new();
		var $stateParams = {filterId:'111-111-111',period: '10'};
		var singleResult = {
			Id: '111-111-111',
			Name: 'newName',
		};


		$httpBackend.whenGET(singleFilter($stateParams.filterId))
			.respond(200, singleResult);
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:$stateParams });

		$httpBackend.flush();
		expect(scope.filterId).toBe("111-111-111");
	}));
	it('should save if default', inject(function ($controller) {
		var scope = $rootScope.$new();
		var $stateParams = {filterId:'000-000-000',period: '10',isDefault:true};
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:$stateParams });

		expect(scope.default).toBe(true);
	}));
	it('should show that no results exists', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();

		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		scope.searchString = 'test';
		scope.results = [];


		expect(scope.noResultsExists()).toEqual(true);
	}));
	it('should not perform rule lookup if default guid', inject(function ($controller,$stateParams) {
		var scope = $rootScope.$new();
		var mockStateParams = {filterId:'00000000-0000-0000-0000-000000000000',period:1,isDefault:true}
		$controller('ResourceplannerFilterCtrl', { $scope: scope,$stateParams:mockStateParams });
		expect(scope.name).toBe('Default')
	}));
	it('should be on first element in results when pressing down key', inject(function ($controller, $stateParams){
		var scope = $rootScope.$new();
		var event = {
			keyCode: 40
		};
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:mockStateParams });
		scope.results = [{Id: '1'},{Id: '2'}];

		scope.onKeydown(event);

		expect(scope.results[0].selected).toBe(true);
	}));
	it('should be on second element in results when pressing down key two times', inject(function ($controller, $stateParams){
		var scope = $rootScope.$new();
		var event = {
			keyCode: 40
		};
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:mockStateParams });
		scope.results = [{Id: '1'},{Id: '2'}];

		scope.onKeydown(event);
		scope.onKeydown(event);

		expect(scope.results[0].selected).toBe(false);
		expect(scope.results[1].selected).toBe(true);
	}));
	it('should be on first element when pressing down key two times and up key ones', inject(function ($controller, $stateParams){
		var scope = $rootScope.$new();
		var eventUp = {
			keyCode: 38
		};
		var eventDown = {
			keyCode: 40
		};
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:mockStateParams });
		scope.results = [{Id: '1'},{Id: '2'}];

		scope.onKeydown(eventDown);
		scope.onKeydown(eventDown);
		scope.onKeydown(eventUp);

		expect(scope.results[0].selected).toBe(true);
		expect(scope.results[1].selected).toBe(false);
	}));
	it('should select current element in results when pressing enter key', inject(function ($controller, $stateParams){
		var scope = $rootScope.$new();
		var eventEnter = {
			keyCode: 13
		};
		var eventDown = {
			down: 40
		};
		$controller('ResourceplannerFilterCtrl', { $scope: scope, $stateParams:mockStateParams });
		scope.selectedResults = [];

		scope.onKeydown(eventDown);
		scope.onKeydown(eventEnter);

		expect(scope.selectedResults.length).toEqual(1);
}));

});
