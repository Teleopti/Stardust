'use strict';
describe('Roles', function() {
	var $q,
		$rootScope,
		$httpBackend,
		Roles;
	var mockPermissionsService = {
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
		duplicateRole: {
			query: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Id: 11, DescriptionText: "text" });
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
		module('wfm');
		module(function ($provide) {
			$provide.service('PermissionsService', function () { return mockPermissionsService; });
		});
	});


	beforeEach(inject(function(_$httpBackend_, _$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
		$httpBackend = _$httpBackend_;
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'mock');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, 'mock');
		}));

	it('should create a role', inject(function (Roles) {
		//create a role with a name
		var roleName = 'role';
		var scope = $rootScope.$new();
		Roles.createRole(roleName);

		scope.$digest();

		expect(Roles.list.length).toBe(1);
		expect(Roles.list[0].Id).toBe(1);
	}));

	it('should copy a role', inject(function (Roles) {
		var roleId = "1";
		var roleName = 'role';
		var scope = $rootScope.$new();
		Roles.createRole(roleName);

		Roles.copyRole(roleId);
		scope.$digest();

		expect(Roles.list.length).toBe(2);
	}));

});