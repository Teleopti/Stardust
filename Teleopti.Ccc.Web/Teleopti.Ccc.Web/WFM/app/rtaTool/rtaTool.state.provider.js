'use strict';

angular.module('wfm.rtaTool')
.config(rtaStates);

function rtaStates($stateProvider) {
  $stateProvider.state('rtaTool', {
    url: '/rtaTool',
    controller: 'RtaToolCtrl',
    templateUrl: 'app/rtaTool/rtaTool.html'
  })
};
