(function() {
  'use strict';

  angular
  .module('wfm.permissions')
  .controller('PermissionsCtrlRefact', PermissionsCtrl);

  PermissionsCtrl.$inject = ['$filter', 'PermissionsServiceRefact', 'permissionsDataService'];

  function PermissionsCtrl($filter, PermissionsServiceRefact, permissionsDataService) {
    var vm = this;
    vm.showCreateModal;
    vm.roleName;

    fetchData();
    vm.createRole = createRole;
    vm.editRole = editRole;
    vm.deleteRole = deleteRole;
    vm.copyRole = copyRole;
    vm.selectRole = selectRole;
    vm.checkMyRole = checkMyRole;
    vm.prepareTree = prepareTree;
    vm.toggleAllFunction = toggleAllFunction;
    vm.unSelectedFunctionsFilter = unSelectedFunctionsFilter;
    vm.selectedFunctionsFilter = selectedFunctionsFilter;
    vm.allFunctionsFilter = allFunctionsFilter;
    vm.selectedOrNot = false;
    vm.selectedRole = {};
    vm.componentFunctions = [];
    vm.listHandler = listHandler;


    //FRÅGA NICKLAS, clona arrayen som vi får in så vi inte skriver övert den jao
    function listHandler(arr) {
      vm.componentFunctions = [].concat(arr);
    }

    function unSelectedFunctionsFilter() {
      var filteredArray = $filter('unSelectedFunctionsFilter')(vm.componentFunctions);
      listHandler(filteredArray);
    }

    function selectedFunctionsFilter() {
      var filteredArray = $filter('selectedFunctionsFilter')(vm.componentFunctions);
      listHandler(filteredArray);
    }

    function allFunctionsFilter(){
      listHandler(vm.applicationFunctions);
    }

    function toggleAllFunction() {
      vm.applicationFunctions[0].IsSelected = !vm.applicationFunctions[0].IsSelected;

      var selectedOrNot = vm.applicationFunctions[0].IsSelected ? true : false;
      vm.selectedOrNot = selectedOrNot;
      toggleSelection(vm.applicationFunctions, selectedOrNot);

      if (vm.selectedRole != null) {
        permissionsDataService.selectAllFunction(vm.selectedRole, vm.applicationFunctions, selectedOrNot);
      }
    }

    function toggleSelection(functions, selectedOrNot) {
      if (functions !== undefined) {
        for (var i = 0; i < functions.length; i++) {
          functions[i].IsSelected = selectedOrNot;
          if (functions[i].ChildFunctions != null && functions[i].ChildFunctions.length > 0) {
            toggleSelection(functions[i].ChildFunctions, selectedOrNot);
          }
        }
      }
    }

    function toggleOrganizationSelecton(businessUnit, selectedOrNot) {
      businessUnit.IsSelected = selectedOrNot;

      for (var i = 0; i < businessUnit.ChildNodes.length; i++) {
        businessUnit.ChildNodes[i].IsSelected = selectedOrNot;
        for (var j = 0; j < businessUnit.ChildNodes[i].ChildNodes.length; j++) {
          businessUnit.ChildNodes[i].ChildNodes[j].IsSelected = selectedOrNot;
        }
      }
    }

    function refreshRoleSelection() {
      for (var i = 0; i < vm.roles.length; i++) {
        vm.roles[i].IsSelected = false;
      }
      vm.roles[0].IsSelected = true;
    }

    function createRole(roleName) {
      var roleData = { Description: roleName };
      PermissionsServiceRefact.roles.save(roleData).$promise.then(function(data) {
        vm.roles.unshift(data);
        vm.selectedRole = data;
        permissionsDataService.setSelectedRole(vm.selectedRole);
        refreshRoleSelection();
        markSelectedRole(vm.selectedRole);
      });
      vm.showCreateModal = false;
      vm.roleName = '';
      toggleSelection(vm.applicationFunctions, false);

      if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
        toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
      }

    }

    function editRole(newRoleName, role) {
      PermissionsServiceRefact.manage.update({ Id: role.Id, newDescription: newRoleName }).$promise.then(function() {
        PermissionsServiceRefact.roles.query().$promise.then(function(data) {
          vm.roles = data;
          var selected = data.find(function(r) {
            return r.Id === role.Id;
          });
          markSelectedRole(selected);
        });
      });
    }

    function checkMyRole(role) {
      if (role.IsMyRole || role.BuiltIn) {
        return false;
      } else {
        return true;
      }
    }

    function deleteRole(role) {
      if (role.BuiltIn) {
        //Hide Delete button for built in roles
        return;
      }

      PermissionsServiceRefact.manage.deleteRole({ Id: role.Id });
      permissionsDataService.setSelectedRole(null);
      var index = vm.roles.indexOf(role);
      vm.roles.splice(index, 1);

      if (role.Id === vm.selectedRole.Id) {
        toggleSelection(vm.applicationFunctions, false);
      }

      if (vm.organizationSelection !== null && vm.organizationSelection.BusinessUnit !== null) {
        toggleOrganizationSelecton(vm.organizationSelection.BusinessUnit, false);
      }

    }

    function copyRole(role) {
      PermissionsServiceRefact.copyRole.copy({ Id: role.Id }).$promise.then(function(data) {
        vm.roles.unshift(data);
        vm.selectedRole = data;
        permissionsDataService.setSelectedRole(vm.selectedRole);
        refreshRoleSelection();
        markSelectedRole(vm.selectedRole);
      });
    }

    function selectRole(role) {
      markSelectedRole(role);

      PermissionsServiceRefact.manage.getRoleInformation({ Id: role.Id }).$promise.then(function(data) {
        vm.selectedRole = data;
        permissionsDataService.setSelectedRole(vm.selectedRole);
        if (vm.selectedRole.AvailableBusinessUnits) {
          matchOrganizationData();
        }

        var functions = vm.applicationFunctions;
        while (functions != null && functions.length > 0) {
          var next = [];
          functions.forEach(function(fn) {
            fn.IsSelected = vm.selectedRole.AvailableFunctions.some(function(afn) {
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

    var findMatchingBu = function () {

    }

    function matchOrganizationData() {
      var sites = vm.organizationSelection.BusinessUnit.ChildNodes;

      for (var i = 0; i < vm.selectedRole.AvailableBusinessUnits.length; i++) {
        if (vm.selectedRole.AvailableBusinessUnits[i].Id == vm.organizationSelection.BusinessUnit.Id) {
          vm.organizationSelection.BusinessUnit.IsSelected = true;
          break;
        }
      }

      if (sites.length > 0) {
        for (var i = 0; i < sites.length; i++) {
          for (var j = 0; j < vm.selectedRole.AvailableSites.length; j++) {
            if (sites[i].Id == vm.selectedRole.AvailableSites[j].Id) {
              sites[i].IsSelected = true;
            } else {
              sites[i].IsSelected = false;
            }
          }
          if (sites[i].ChildNodes.length > 0 && vm.selectedRole.AvailableTeams.length > 0) {
          for (var a = 0; a < vm.selectedRole.AvailableTeams.length; a++) {
              if (sites[i].ChildNodes[a].Id == vm.selectedRole.AvailableTeams[a].Id){
                sites[i].ChildNodes[a].IsSelected = true;
              } else {
                sites[i].ChildNodes[a].IsSelected = false;
              }
            }
          }
        }
      }
    }

    function matchOrganizationData2() {
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
              vm.organizationSelection.BusinessUnit.ChildNodes[i].IsSelected = true;
            } else {
              vm.organizationSelection.BusinessUnit.ChildNodes[i].IsSelected = false;
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
      if (appFunctions != null && appFunctions.length > 1) {
        appFunctions[1].IsOpen = true;
        appFunctions[0].IsHidden = true;
      }
    }

    function fetchData() {
      PermissionsServiceRefact.roles.query().$promise.then(function(data) {
        vm.roles = data;
      });
      PermissionsServiceRefact.applicationFunctions.query().$promise.then(function(data) {
        vm.applicationFunctions = data;
        listHandler(vm.applicationFunctions);
        prepareTree(vm.applicationFunctions);
      });
      PermissionsServiceRefact.organizationSelection.get().$promise.then(function(data) {
        vm.organizationSelection = data;
      });
    }

  }
})();
