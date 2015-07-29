(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService','Roles',
		function ($scope, $filter, RoleDataService, Roles) {
			$scope.organization = {};

			$scope.$watch(function () { return RoleDataService.organization; },
				function () {
					$scope.organization = RoleDataService.organization;
				}
			);
			var traverseNodes = function (node) {
			    for (var i = 0; i < node.length; i++) {
			        if (node[i].ChildNodes.length === 0)
			            node[i].selected = false;
			        else {
			            node[i].selected = false;
			            $scope.loopAround(node[i]);
			        }
			    }

			}
			$scope.toggleOrganizationSelection = function (node) {
			    if (Roles.selectedRole.BuiltIn === false) {
			        if (node.selected) {
			            RoleDataService.deleteAvailableData($scope.selectedRole, node.Type, node.Id).
			            then(function() {
			                node.selected = false;

			                $scope.loopAround = function(a) {
			                    var childs = a.ChildNodes;
			                    traverseNodes(childs);

			                }
			                var rootNode = node.ChildNodes;
			                traverseNodes(rootNode);
			            });
			        } else {
			            node.selected = true;
			            RoleDataService.assignOrganizationSelection($scope.selectedRole, node.Type, node.Id).then(function () {
			            });
			        }
			    }
			};
			$scope.changeOption = function (option) {
			    RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
			}
		}
	]);

})();