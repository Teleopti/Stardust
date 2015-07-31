'use strict';

describe('Outbound Chart Service Test', function() {

    var $q,
        $rootScope,
        $httpBackend,
        $translate,
        $filter,
        target;

    beforeEach(function() {
        module('outboundServiceModule');      
    });

    beforeEach(inject(function (_$httpBackend_, _$q_, _$rootScope_, _$translate_, _$filter_, _outboundChartService_) {
        $q = _$q_;
        $rootScope = _$rootScope_;
        $httpBackend = _$httpBackend_;
        $translate = _$translate_;
        $filter = _$filter_;
        $httpBackend.resetExpectations();
        target = _outboundChartService_;
    }));

    it('Get campaign visualization should fetch data correctly', function () {
        
        target.buildGraphDataSeqs = function (v) { return v; };

        $httpBackend.whenGET('../api/Outbound/Campaign/Visualization/1')
            .respond(200, JSON.stringify({
                Dates: '2015-07-31'
            }));

        var resultData;

        target.coreGetCampaignVisualization('1', function (data) {
            resultData = data;
        });

        $httpBackend.flush();

        expect(resultData).toBeDefined();
        expect(resultData.Dates).toEqual('2015-07-31');
    });
});