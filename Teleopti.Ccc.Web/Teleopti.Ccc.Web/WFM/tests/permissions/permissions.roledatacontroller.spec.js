'use strict';
describe('DataController', function() {

	var $q,
		$rootScope;

	var mockRoleDataService = {
		assignOrganizationSelection:function(selectedRole, type, id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		},
		deleteAvailableData: function (selectedRole, type, id) {
			var deferred = $q.defer();
			deferred.resolve();
			return deferred.promise;
		}
	};
    var mockRoles = {
        selectedRole:{
        BuiltIn: false
    }
}

	beforeEach(function () {
		module('wfm.permissions');
	});

	beforeEach(inject(function (_$q_, _$rootScope_) {
		$q = _$q_;
		$rootScope = _$rootScope_;
	}));

	it('should toggle the selected property of node from false to true', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { selected: false };

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.selected).toBe(true);
	}));

	it('should toggle the selected property of node from true to false', inject(function ($controller) {
		var scope = $rootScope.$new();
		$controller('RoleDataController', { $scope: scope, RoleDataService: mockRoleDataService, Roles: mockRoles });
		var node = { selected: true, ChildNodes: [] };

		scope.toggleOrganizationSelection(node);
		scope.$digest();

		expect(node.selected).toBe(false);
	}));
});