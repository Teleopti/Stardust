(function () {
  'use strict';
  angular.module ('wfm.people').config (stateConfig);

  function stateConfig ($stateProvider) {
    $stateProvider.state ('people', {
      url: '/people',
      template: '<ng2-people></ng2-people>',
    });
  }
}) ();
