'use strict';
describe('PermissionsCtrl', function () {
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

	var mockPermissionsService = { 
		roles: {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Id: 1, DescriptionText: 'text' });
				return { $promise: queryDeferred.promise };
			},
			get: function() {
				return [];
			}
		},
		applicationFunctions: {
			query: function() {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([
				{
					FunctionDescription: "desc",
					LocalizedFunctionDescription: "",
					FunctionCode: "",
					IsDisabled: false,
					FunctionId: 1,
					ChildFunctions:[]
			}]);
				return { $promise: queryDeferred.promise };
			}
		},
		organizationSelections: {
			query: function () {
				var queryDeferred = $q.defer();
				return { $promise: queryDeferred.promise };
			}
		},
		postFunction: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({});
				return { $promise: queryDeferred.promise };
			}
		},
		rolesPermissions: {
			query: function () {
				var result = {AvailableFunctions:[
				{
					Id:"1", FunctionCode:"", FunctionPath:"", LocalizedFunctionDescription:""
				}],
					AvailableBusinessUnits: [],
					AvailableSites: [],
					AvailableTeams: []
				};
				var queryDeferred = $q.defer();
				queryDeferred.resolve(result);
				return { $promise: queryDeferred.promise };
			}
		}
	};

	it('creates a role in the list', inject( function ($controller) {
		var scope = $rootScope.$new();
		
		$controller('PermissionsCtrl', { $scope: scope, Permissions: mockPermissionsService });
		

		scope.roleName = 'name';
		scope.createRole();
		scope.$digest(); // this is needed to resolve the promise
		
		expect(scope.roles.length).toEqual(1); 
	}));

	it('selects a function for a role', inject(function($controller) {
		var functionNode = {
			$modelValue: { selected: false },
			$parentNodeScope: {
				$modelValue: { hasSelectedChildren: 0 }
			}
	};
		var scope = $rootScope.$new();
		$controller('PermissionsCtrl', { $scope: scope, Permissions: mockPermissionsService });

		scope.toggleFunctionForRole(functionNode);
		scope.$digest();

		expect(functionNode.$modelValue.selected).toBe(true);
	}));

	it('updates functions list when selected functions for a role is updated from the service ', inject(function ($controller) {

		var scope = $rootScope.$new();
		$controller('PermissionsCtrl', { $scope: scope, Permissions: mockPermissionsService });

		scope.showRole("roleId");
		scope.$digest();

		var functionSelected;
		scope.functionsDisplayed.forEach(function(item) {
			if (item.selected) { //FIXME
				functionSelected = item;
			}
		});
		expect(functionSelected).not.toBe(null);
		expect(functionSelected.FunctionId).toBe(1);
	}));

	it('checks if a function has children selected', inject(function ($controller) {
		var parentFunction = {};
		var scope = $rootScope.$new();

		var mockPermissionsService2 = {
			roles: {
				post: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve({ Id: 1, DescriptionText: 'text' });
					return { $promise: queryDeferred.promise };
				},
				get: function () {
					return [];
				}
			},
			applicationFunctions: {
				query: function () {
					var queryDeferred = $q.defer();
					queryDeferred.resolve([
					{
						FunctionDescription: "desc",
						LocalizedFunctionDescription: "",
						FunctionCode: "",
						IsDisabled: false,
						FunctionId: 1,
						ChildFunctions: [
							{
								FunctionDescription: "desc",
								LocalizedFunctionDescription: "",
								FunctionCode: "",
								IsDisabled: false,
								FunctionId: 2,
								ChildFunctions: []
							}
						]
					}]);
					return { $promise: queryDeferred.promise };
				}
			},
			organizationSelections: {
				query: function () {
					var queryDeferred = $q.defer();
					return { $promise: queryDeferred.promise };
				}
			},
			rolesPermissions: {
				query: function () {
					var result = {
						AvailableFunctions: [
						{
							Id: "2", FunctionCode: "", FunctionPath: "", LocalizedFunctionDescription: ""
						}],
						AvailableBusinessUnits: [],
						AvailableSites: [],
						AvailableTeams: []
					};
					var queryDeferred = $q.defer();
					queryDeferred.resolve(result);
					return { $promise: queryDeferred.promise };
				}
			}
		};


		$controller('PermissionsCtrl', { $scope: scope, Permissions: mockPermissionsService2 });
		scope.showRole("roleId");

		scope.$digest();
		
		expect(scope.functionsDisplayed[0].nmbSelectedChildren).toEqual(1);
	}));
});
