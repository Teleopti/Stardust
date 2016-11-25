(function() {
  'use strict';

  angular
  .module('wfm.permissions')
  .filter('selectedFunctionsFilter', selectedFunctionsFilter)
  .filter('unSelectedFunctionsFilter', unSelectedFunctionsFilter);

  function selectedFunctionsFilter() {
    return filter;

    function findSelected (func) {
      return func.IsSelected === true;
    }

    function filter(appFunctions) {
      var temp = appFunctions.filter(findSelected);
      deleteStuff(temp);
      return temp;
    }

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

  }

  function unSelectedFunctionsFilter(){
    return filter;


    function filter(appFunctions) {
      var temp = appFunctions.filter(findUnselectedOrParents);
      deleteStuff(temp);

      return temp;
    }
    function deleteStuff(temp) {
      for (var i = 0; i < temp.length; i++) {
          if (temp[i].IsSelected && temp[i].ChildFunctions.length < 1) {
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

    function findUnselectedOrParents (func) {
      return func.IsSelected === false || func.ChildFunctions.length > 0;
    }

  }

})();
