(function() {
	'use strict';
	angular.module('wfm.permissions')
        .controller('PermissionsCtrl', [
			'$scope', '$filter', 'PermissionsService', 'Roles', '$stateParams',
			function($scope, $filter, Permissions, Roles, $stateParams) {
				$scope.list = [];
				$scope.roleName = null;
				$scope.roleDetails = 'functionsAvailable';
				
				$scope.functionsFlat = [];
				$scope.dataFlat = [];
				$scope.selectedRole = Roles.selectedRole;
				$scope.selectedDataToggle = false;
				$scope.unselectedDataToggle = false;
				$scope.roles = [];

				$scope.editing = false;
				$scope.builtInCheck = false;

				$scope.$watch(function () { return Roles.selectedRole; },
			   function (newSelectedRole) {
			   	if (!newSelectedRole.Id) return;
			  
			   	$scope.builtInCheck = newSelectedRole.BuiltIn;
			   }
		   );

				$scope.createRole = function() {
				    Roles.createRole($scope.roleName).then(function () {
						//here a notice
				        $scope.reset();
				        $scope.showRole($scope.roles[0]);
				    });
				};
				$scope.test = function (string) {
					console.log('blur');
				};
				$scope.reset = function() {
					$scope.roleName = "";
				};

				$scope.copyRole = function(role) {
					Roles.copyRole(role.Id).then(function () {
						$scope.showRole(role);
					});
				};

				$scope.removeRole = function(role) {
					if (confirm('Are you sure you want to delete this?')) {
						Roles.removeRole(role);
					}
				};

				$scope.updateRole = function(role) {
					Permissions.manageRole.update({ Id: role.Id, newDescription: role.DescriptionText });

				};

				$scope.submitRole = function(role){
					if (role.DescriptionText != '') {
						role.editing = false; 
						Permissions.manageRole.update({ Id: role.Id, newDescription: role.DescriptionText });
					}else{
                        role.DescriptionText = role.Name;
                        role.editing = false;

                    }
				};

				$scope.showRole = function (role) {
					if (role.Id === $scope.selectedRole) return;
					Roles.selectRole(role);
					$scope.selectedRole = role.Id;
					$scope.builtInCheck = role.BuiltIn;
					$scope.roles.forEach(function (item) {
						$scope.cancelEdit(item);
					});

				};

				$scope.nbSelected = function($nodes) {
					var nb = 0;
					$nodes.forEach(function($node) {
						if ($node.$modelValue.selected)
							nb++;
					});
					return nb;
				};

				$scope.doubleClickEdit = function (edit, role) {
					if ($scope.builtInCheck) { return; }

					

					//console.log(role);
					role.editing = true;
				};

				$scope.cancelEdit = function (role) {
					if (role.DescriptionText != '') {
						role.editing = false;
					}
				};


				Roles.refresh().$promise.then(function (result) {
					$scope.roles = result;
					
					if ($stateParams.id != null) {
						for (var i = 0; i < result.length; i++) {
							if ($stateParams.id == result[i].Id) {
								$scope.showRole(result[i]);
							}
						}
					} else {
						if (result.length > 0)
							$scope.showRole(result[0]);
					}
				});
			}
		]
	);
})();



