(function () {
  'use strict';

  angular
    .module('wfm.timebank')
    .factory('timebankService', timebankService);

  timebankService.inject = [];
  function timebankService() {

    var people = [
      {
        Name: 'Kalle',
        Id: 'd0fee6bb-d1e0-4265-a08b-5229d76af0e3',
        Contract: {
          Name: 'Part-time',
          Id: '2229aff1-a625-4266-b1d7-72f400872efb'
        }
      }
    ];

    var contracts = [
      {
        Name: 'part-time',
        Id: '2229aff1-a625-4266-b1d7-72f400872efb'
      }
    ]

    function getPeople() {
      return people;
    }

    function getContracts() {
      return contracts;
    }


    var service = {
      getPeople: getPeople,
      getContracts: getContracts
    };

    return service;

  }
})();