(function() {
    'use strict';

    angular
        .module('wfm.permissions')
        .filter('showOnlySelectedFunctionsFilter', showOnlySelectedFunctionsFilter);

    function showOnlySelectedFunctionsFilter() {
        return filter;


        function filter(appFunctions) {
          var filteredArray = [];
          itemFinder(appFunctions, filteredArray);
          return filteredArray;
        }

        function itemFinder(arr, filteredArray){
          for (var i = 0; i < arr.length; i++) {
            if (arr[i].IsSelected){
              filteredArray.push(arr[i]);
              if (arr[i].ChildFunctions != null && arr[i].ChildFunctions.length > 0){
                itemFinder(arr[i].ChildFunctions, filteredArray);
              }
            }
        }
      }
    }
})();
