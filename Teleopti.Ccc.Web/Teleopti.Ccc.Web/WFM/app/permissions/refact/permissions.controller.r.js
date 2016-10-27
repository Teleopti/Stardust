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

        function findAllMatchingFunctions(selectedRole) {
              vm.applicationFunctions = vm.applicationFunctions.map(function (fn) {
                    fn.IsSelected = selectedRole.AvailableFunctions.some(function (af) {
                        return fn.FunctionId == af.Id;
                    });

                    if (fn.ChildFunctions.length > 0) {
                        console.log(fn);
                        fn.ChildFunctions[0].IsSelected = true;
                    }
                    return fn;
                });
        }
    
        function selectRole(role) {
            markSelectedRole(role);
            //HELA VÄGEN JAO: SÄTT ISSELECTED BRah

            PermissionsServiceRefact.manage.getRoleInformation({ Id: role.Id }).$promise.then(function (data) {
                vm.selectedRole = data;
                findAllMatchingFunctions(vm.selectedRole);
            });
        }
            
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

