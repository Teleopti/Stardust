'use strict';

angular.module('wfm.rtaTracer')
  .config(rtaTracer);

function rtaTracer($stateProvider) {
  $stateProvider.state('rtaTracer', {
    url: '/rtaTracer',
    controller: 'RtaTracerController as vm',
    templateUrl: 'app/rtaTracer/rtaTracer.html'
    
  })
}
