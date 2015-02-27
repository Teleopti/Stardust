'use strict';
describe('PermissionsCtrl', function () {
	var $q,
		$rootScope;

	beforeEach(module('wfm'));

	beforeEach(inject(function (_$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
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
				return { $promise: queryDeferred.promise };
			}
		},
		organizationSelections: {
			query: function () {
				var queryDeferred = $q.defer();
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
});
