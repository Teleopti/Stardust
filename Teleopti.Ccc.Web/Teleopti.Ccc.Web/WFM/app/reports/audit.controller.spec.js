'use strict';
describe('AuditTrailController', function () {
    var $httpBackend,
        $controller;

    beforeEach(function () {
        module('wfm.reports');
        module('externalModules');
    });

    beforeEach(inject(function (_$httpBackend_, _$controller_ ) {
        $httpBackend = _$httpBackend_;
        $controller = _$controller_;
    }));


    // it('should', function () {
    //     var vm = $controller('AuditTrailController');
    //     expect(true).toBe(false);
    // });
});
