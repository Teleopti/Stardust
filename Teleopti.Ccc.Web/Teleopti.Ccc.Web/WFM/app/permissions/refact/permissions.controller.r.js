(function () {
    'use strict';

    angular
        .module('wfm.permissions')
        .controller('PermissionsCtrlRefact', PermissionsCtrl);

    PermissionsCtrl.$inject = ['PermissionsServiceRefact'];

    function PermissionsCtrl(PermissionsServiceRefact) {
        var vm = this;
        vm.showCreateModal;
        vm.roleName;

        fetchData();
        vm.createRole = createRole;
        vm.editRole = editRole;
        vm.deleteRole = deleteRole;
        vm.copyRole = copyRole;
        vm.selectRole = selectRole;
        vm.checkMyRole  = checkMyRole;
        vm.prepareTree = prepareTree;
        vm.toggleAllFunction = toggleAllFunction;
        vm.selectedOrNot = false;

        function toggleAllFunction () {
          vm.applicationFunctions[0].IsSelected = !vm.applicationFunctions[0].IsSelected;


          var selectedOrNot = vm.applicationFunctions[0].IsSelected ? true : false;
          vm.selectedOrNot = selectedOrNot;
          toggleSelection(vm.applicationFunctions, selectedOrNot);
        }

        function toggleSelection(functions, selectedOrNot){
          for (var i = 0; i < functions.length; i++) {
            functions[i].IsSelected = selectedOrNot;
            if(functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
              toggleSelection(functions[i].ChildFunctions, selectedOrNot);
            }
          }
        }

        function createRole(roleName) {
            var roleData = { Description: roleName };
            PermissionsServiceRefact.roles.save(roleData).$promise.then(function (data) {
                vm.roles.unshift(data);
            });
            vm.showCreateModal = false;
            vm.roleName = '';
        }

        function editRole(newRoleName, role) {
            PermissionsServiceRefact.manage.update({ Id: role.Id, newDescription: newRoleName }).$promise.then(function () {
                PermissionsServiceRefact.roles.query().$promise.then(function (data) {
                    vm.roles = data;
                    var selected = data.find(function (r) {
                        return r.Id === role.Id;
                    });
                    markSelectedRole(selected);
                });
            });
        }

        function checkMyRole(role){
            if(role.IsMyRole || role.BuiltIn){
                return false;
            }else{
                return true;
            }
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

            PermissionsServiceRefact.manage.getRoleInformation({ Id: role.Id }).$promise.then(function (data) {
                vm.selectedRole = data;
                if (vm.selectedRole.AvailableBusinessUnits) {
                    matchOrganizationData();
                }

                var functions = vm.applicationFunctions;
                while (functions != null && functions.length > 0) {
                    var next = [];
                    functions.forEach(function (fn) {
                        fn.IsSelected = vm.selectedRole.AvailableFunctions.some(function (afn) {
                            return fn.FunctionId === afn.Id;
                        });
                        if (fn.ChildFunctions != null) {
                            next = next.concat(fn.ChildFunctions);
                        }
                    });

                    functions = next;
                }
            });
        }

        function matchOrganizationData() {
            for (var i = 0; i < vm.selectedRole.AvailableBusinessUnits.length; i++) {
                if (vm.selectedRole.AvailableBusinessUnits[i].Id == vm.organizationSelection.BusinessUnit.Id) {
                    vm.organizationSelection.BusinessUnit.IsSelected = true;
                    break;
                }
            }

            if (vm.organizationSelection.BusinessUnit.ChildNodes.length > 0) {
                for (var i = 0; i < vm.organizationSelection.BusinessUnit.ChildNodes.length; i++) {
                    for (var j = 0; j < vm.selectedRole.AvailableSites.length; j++) {
                        if (vm.organizationSelection.BusinessUnit.ChildNodes[i].Id == vm.selectedRole.AvailableSites[j].Id) {
                            vm.organizationSelection.BusinessUnit.ChildNodes[i].IsSelected = false;
                        } else {
                            vm.organizationSelection.BusinessUnit.ChildNodes[i].IsSelected = true;
                        }
                    }
                }
            }

        }

        var previously = null;
        function markSelectedRole(role) {
            if (previously != null) {
                previously.IsSelected = false;
            }
            role.IsSelected = true;
            previously = role;
        }

        function prepareTree(appFunctions) {
          if (appFunctions != null && appFunctions.length > 1){
              appFunctions[1].IsOpen = true;
              appFunctions[0].IsHidden = true;
          }
        }

        function fetchData() {
            PermissionsServiceRefact.roles.query().$promise.then(function (data) {
                vm.roles = data;
            });
            PermissionsServiceRefact.applicationFunctions.query().$promise.then(function (data) {
                vm.applicationFunctions = data;
                prepareTree(vm.applicationFunctions);
            });
            PermissionsServiceRefact.organizationSelection.get().$promise.then(function (data) {
                vm.organizationSelection = data;
            });
        }

    }
})();
