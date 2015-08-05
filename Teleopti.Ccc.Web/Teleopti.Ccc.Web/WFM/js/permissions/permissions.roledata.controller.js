(function () {
	'use strict';

	angular.module('wfm.permissions').controller('RoleDataController', [
		'$scope', '$filter', 'RoleDataService','Roles',
	        function($scope, $filter, RoleDataService, Roles) {
	            $scope.organization = { DynamicOptions: []};

	            $scope.dynamicOptionSelected = null;
	            $scope.$watch(function() { return Roles.selectedRole; },
	                function(newSelectedRole) {
	                    if (!newSelectedRole.Id) return;                  
	                    RoleDataService.refreshpermissions(newSelectedRole.Id);
	                }
	            );

	            $scope.$watch(function () { return RoleDataService.organization },
                   function (organization) {
                       $scope.organization = organization;
                       $scope.dynamicOptionSelected = RoleDataService.dynamicOptionSelected;;
                   }
           );  
	            $scope.$watch(function () { return RoleDataService.dynamicOptionSelected; },
                       function (option) {
                           $scope.dynamicOptionSelected = option;      //fixme                   
                       }
                    );
			
			var traverseNodes = function (node) {
			    for (var i = 0; i < node.length; i++) {
			        if (node[i].ChildNodes.length === 0)
			            node[i].selected = false;
			        else {
			            node[i].selected = false;
			            traverseNodes(node[i].ChildNodes);
			        }
			    }

			}
			$scope.toggleOrganizationSelection = function (node) {

			    if (Roles.selectedRole.BuiltIn) return;
			  
			    var dataNode = node.$modelValue;
			    if (dataNode.selected) {
			        RoleDataService.deleteAvailableData($scope.selectedRole, dataNode.Type, dataNode.Id).
			        then(function() {
			            dataNode.selected = false;      
			            // $scope.loopAround = function(a) {
			                // traverseNodes(a.ChildNodes);
			            //  }			         
			            traverseNodes(dataNode.ChildNodes);
			        });
			    } else {
			        dataNode.selected = true;
                        
			        var dataNodes = [];
			        dataNodes.push({ type: dataNode.Type, id: dataNode.Id });
			        if (node.$parentNodeScope !== null) {
			            var parent = node.$parentNodeScope;
			            while (parent !== null) {
			                if (!parent.$modelValue.selected) {
			                    parent.$modelValue.selected = true;
			                    dataNodes.push({type:parent.$modelValue.Type, id: parent.$modelValue.Id });
			                }
			                parent = parent.$parentNodeScope;
			            }
			        }
			        RoleDataService.assignOrganizationSelection($scope.selectedRole, dataNodes);
			    }
			    
			};
			$scope.changeOption = function (option) {
			    RoleDataService.assignAuthorizationLevel($scope.selectedRole, option);
			}
		}
	]);

})();