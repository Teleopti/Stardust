'use strict';
describe('PermissionsCtrl', function () {
	var scope, Roles, $location;
	//$scope, $stateParams, $filter, Roles, OrganizationSelections, ApplicationFunctions, DuplicateRole, RolesPermissions, ManageRole, AssignFunction, AssignOrganizationSelection

	beforeEach(module('wfmCtrls'), function ($provide) {
		var mockRolesService = {};
		var mocOrganizationSelectionsService = {};
		var mockApplicationFunctionsService = {};
		var mockDuplicateRoleService = {};
		var mockRolesPermissionsService = {};
		var mockManageRoleService = {};
		var mockAssignFunctionService = {};
		var mockAssignOrganizationSelectionService = {};
		$provide.value('Roles', mockRolesService);
		$provide.value('OrganizationSelections', {});
		$provide.value('ApplicationFunctions', {});
		$provide.value('DuplicateRole', {});
		$provide.value('RolesPermissions', {});
		$provide.value('ManageRole', {});
		$provide.value('AssignFunction', {});
		$provide.value('AssignOrganizationSelection', {});

		inject(function($q) {
			mockRolesService.post = function() {
				var defer = $q.defer();
				result.Id = 1;
				result.DescriptionText = 'role name';
				defer.resolve(result);
				return defer.promise;
			};
		});
	});

	beforeEach(inject(function ($controller, $rootScope, _$location_, _Roles_, _OrganizationSelections_) { 
		scope = $rootScope.$new();
		Roles = _Roles_;
		$location = _$location_;
		$controller('PermissionsCtrl', { $scope: scope, $location: $location, Roles: Roles }); //wait for controller refacto

		scope.$digest();
	}));

	describe('$scope.createRole', function () {
		it('creates a role in the list', function () {
			scope.roleName = 'name';
			scope.createRole();
			expect(scope.roles.length).toEqual(1);
		});
	});
});