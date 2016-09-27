'use strict';
describe('PermissionsCtrl', function () {
	var $q,
	    $rootScope,
	    $httpBackend,
	    	rolesList = [{ Id: 1, DescriptionText: 'text' }];
	var mockPermissionsService = {
		roles: {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Id: 1, DescriptionText: 'text' });
				return { $promise: queryDeferred.promise };
			},
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve(rolesList);
				return { $promise: queryDeferred.promise };
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
					ChildFunctions: []
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
				var result = {
					AvailableFunctions: [
					{
						Id: "1", FunctionCode: "", FunctionPath: "", LocalizedFunctionDescription: ""
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

	beforeEach(function () {
		module('wfm.permissions');
		module('externalModules');
		module(function ($provide) {
			$provide.service('PermissionsService', function() { return mockPermissionsService; });
		});
	});

	beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
	}));


	it('creates a role in the list', inject(function($controller) {
		var scope = $rootScope.$new();

		$controller('PermissionsCtrl', { $scope: scope, PermissionsService: mockPermissionsService });

		scope.roleName = 'name';
		scope.createRole();
		scope.$digest();

		expect(scope.roles.length).toEqual(2);
	}));

	it('permission is loaded without roles', inject(function ($controller) {
		var scope = $rootScope.$new();

		rolesList = [];
		$controller('PermissionsCtrl', { $scope: scope, PermissionsService: mockPermissionsService });

		scope.$digest();

		expect(scope.roles.length).toEqual(0);
	}));
});
