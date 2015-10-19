'use strict';
describe('Roles', function() {
    var $q,
        $rootScope,
        $httpBackend;
	var mockPermissionsService = {
		roles: {
			post: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve({ Id: 1, DescriptionText: 'text' });
				return { $promise: queryDeferred.promise };
			},
			get: function () {
				var queryDeferred = $q.defer();
				queryDeferred.resolve([{ Id: 1, DescriptionText: 'text' }]);
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
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'en');
		$httpBackend.expectGET("../api/Global/User/CurrentUser").respond(200, {Language: 'en', DateFormat: 'en'});
		$httpBackend.expectGET("js/forecasting/html/forecasting.html").respond(200);
		$httpBackend.expectGET("../api/Global/Language?lang=en").respond(200, 'en');

		}));

	it('should create a role', function(done) {
		inject(function(Roles) {
			var roleName = 'role';
			var scope = $rootScope.$new();
			Roles.createRole(roleName);

			scope.$digest();

			var testRoles = function(roles) {
				expect(roles.length).toBe(2);
				expect(roles[0].Id).toBe(1);
			};

			var failTest = function(error) {
				expect(error).toBeUndefined();
			};

			Roles.list.$promise
				.then(testRoles)
				.catch(failTest)
				.finally(done);

			$httpBackend.flush();
		});
	});

	it('should copy a role', function(done) {
		inject(function(Roles) {
			var roleId = "1";
			var roleName = 'role';
			var scope = $rootScope.$new();
			Roles.createRole(roleName);

			Roles.copyRole(roleId);
			scope.$digest();

			var testRoles = function (roles) {
				expect(roles.length).toBe(3);
			};

			var failTest = function (error) {
				expect(error).toBeUndefined();
			};

			Roles.list.$promise
				.then(testRoles)
				.catch(failTest)
				.finally(done);

			$httpBackend.flush();
		});
	});

});
