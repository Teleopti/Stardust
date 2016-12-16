(function () {
  'use strict';

  angular
    .module('wfm.permissions')
    .filter('functionsFilter', functionsFilter)
    .filter('dataFilter', dataFilter);

  function dataFilter() {
    var filter = this;

    function deleteStuff(temp, selectedOrNot) {
      for (var i = 0; i < temp.length; i++) {
        if (temp[i].IsSelected == selectedOrNot) {
          temp.splice(i, 1);
          i--;
        }
        else {
          if (temp[i].ChildNodes != null && temp[i].ChildNodes.length > 0) {
            deleteStuff(temp[i].ChildNodes, selectedOrNot)
          }
        }
      }
    }

    filter.unselected = function (orgData) {
      var selectedBu = {};

      if (orgData.IsSelected) {
        selectedBu = Object.assign({}, orgData);
        deleteStuff(selectedBu.ChildNodes, true);
        return selectedBu;
      } else {
        return orgData;
      }

    }

    filter.selected = function (orgData) {
      var selectedBu = {};

      if (orgData.IsSelected) {
        selectedBu = Object.assign({}, orgData);
        deleteStuff(selectedBu.ChildNodes, false);
        return selectedBu;
      } else {
        return selectedBu;
      }
    }

    return filter;
  }

  function functionsFilter() {
    var filter = this;

    filter.selected = function (appFunctions) {
      var temp = appFunctions.filter(findSelected);
      deleteStuff(temp);
      return temp;
    };

    filter.unselected = function (appFunctions) {
      var temp = appFunctions.filter(findUnselectedOrParents);
      deleteStuff2(temp);
      return temp;
    };

    function deleteStuff(temp) {
      for (var i = 0; i < temp.length; i++) {
        if (!temp[i].IsSelected) {
          temp.splice(i, 1);
          i--;
        }
        else {
          if (temp[i].ChildFunctions.length > 0) {
            deleteStuff(temp[i].ChildFunctions)
          }
        }
      }
    }

    function deleteStuff2(temp) {
      for (var i = 0; i < temp.length; i++) {
        if (temp[i].IsSelected && temp[i].ChildFunctions.length < 1) {
          temp.splice(i, 1);
          i--;
        }
        else {
          if (temp[i].ChildFunctions.length > 0) {
            deleteStuff2(temp[i].ChildFunctions)
          }
        }
      }
    }

    function findSelected(func) {
      return func.IsSelected === true;
    }

    function findUnselectedOrParents(func) {
      return func.IsSelected === false || func.ChildFunctions.length > 0;
    }

    return filter;
  }

})();



