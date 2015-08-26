(function() {

    'use strict';

    angular.module('wfm.outbound').directive('formLocator', function () {
        return {
            restrict: 'A',
            link: function (scope, elem, attr) {
                var identifier = attr['name'];
                scope.$emit('formLocator.' + identifier);
            }
        }
    });
})();