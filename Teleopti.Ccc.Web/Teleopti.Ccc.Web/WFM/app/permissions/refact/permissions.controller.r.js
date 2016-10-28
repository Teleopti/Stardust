(function () {
    'use strict';

    angular
        .module('wfm.permissions')
        .controller('PermissionsCtrlRefact', PermissionsCtrl);

    PermissionsCtrl.$inject = ['PermissionsServiceRefact'];

    function PermissionsCtrl(PermissionsServiceRefact) {
        var vm = this;

        fetchData();
        vm.createRole = createRole;
        vm.editRole = editRole;
        vm.deleteRole = deleteRole;
        vm.copyRole = copyRole;
        vm.selectRole = selectRole;

        function createRole(roleName) {
            var roleData = { Description: roleName };
            PermissionsServiceRefact.roles.save(roleData).$promise.then(function (data) {
                vm.roles.unshift(data);
            });
        }

        function editRole(newRoleName, id) {
            PermissionsServiceRefact.manage.update({ Id: id, newDescription: newRoleName });
        }

        function deleteRole(role) {
            if (role.BuiltIn) {
                //Hide Delete button for built in roles
                return;
            }
            PermissionsServiceRefact.manage.deleteRole({ Id: role.Id });
            var index = vm.roles.indexOf(role);
            vm.roles.splice(index, 1);
        }

        function copyRole(role) {
            PermissionsServiceRefact.copyRole.copy({ Id: role.Id }).$promise.then(function (data) {
                vm.roles.unshift(data);
            });
        }

        function selectRole(role) {
            markSelectedRole(role);
            //HELA VÄGEN JAO: SÄTT ISSELECTED BRah

            PermissionsServiceRefact.manage.getRoleInformation({ Id: role.Id }).$promise.then(function (data) {
                vm.selectedRole = data;
                //findAllMatchingFunctions(vm.selectedRole);

                for (var i = 0; i < vm.selectedRole.AvailableFunctions.length; i++) {
                  //console.log('matchId',i,vm.selectedRole.AvailableFunctions[i].Id );
                  parseTreeJson(vm.applicationFunctions, vm.selectedRole.AvailableFunctions[i].Id);
                }
            });
        }

        var parseTreeJson = function(treeNodes, matchId){
          if (!treeNodes || !treeNodes.length) return;
           for (var i = 0, len = treeNodes.length; i < len; i++) {

                var childs = treeNodes[i].ChildFunctions;
                //console.log('find all id', i, treeNodes[i].FunctionId);

                if (treeNodes[i].FunctionId == matchId) {
                  treeNodes[i].IsSelected = true;
                } else if(!treeNodes[i].IsSelected) {
                  treeNodes[i].IsSelected = false;
                }
                if(childs && childs.length > 0){
                  parseTreeJson(childs,matchId);
                }
           }
           //console.log('hello tree',vm.applicationFunctions);
        };

        var previously = null;
        function markSelectedRole(role) {
             if (previously != null) {
                previously.IsSelected = false;
            }
            role.IsSelected = true;
            previously = role;
        }

        function fetchData() {
            PermissionsServiceRefact.roles.query().$promise.then(function (data) {
                vm.roles = data;
            });
            PermissionsServiceRefact.applicationFunctions.query().$promise.then(function (data) {
                vm.applicationFunctions = data;
            });
            PermissionsServiceRefact.organizationSelection.get().$promise.then(function (data) {
                vm.organizationSelection = data;
            });
        }

    }
})();
