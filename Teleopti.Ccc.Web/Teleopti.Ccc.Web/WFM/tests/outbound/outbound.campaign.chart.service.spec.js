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

    it('Basic graph data should be mapped correctly', function() {
        var input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 50,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        };

        var resultData = target.coreMapGraphData(input);
        expect(resultData.dates).toEqual('2015-07-31');
        expect(resultData.plans).toEqual(30);
        expect(resultData.unscheduledPlans).toEqual(0);
        expect(resultData.schedules).toEqual(50);
        expect(resultData.rawBacklogs).toEqual(100);
        expect(resultData.progress).toEqual(100);

    });

    it('Overscheduled person hours should be mapped correctly', function() {
        var input, resultData;

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 50,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        };

        resultData = target.coreMapGraphData(input);
        expect(resultData.overDiffs).toEqual(20);

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 20,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        }

        resultData = target.coreMapGraphData(input);
        expect(resultData.overDiffs).toEqual(0);

    });

    it('Underscheduled person hours should be mapped correctly', function () {
        var input, resultData;

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 50,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        };

        resultData = target.coreMapGraphData(input);
        expect(resultData.underDiffs).toEqual(0);

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 20,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        }

        resultData = target.coreMapGraphData(input);
        expect(resultData.underDiffs).toEqual(10);

    });

    it('Calculated backlog should be mapped correctly when there is underschedule', function () {
        var input, resultData;

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 50,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        };

        resultData = target.coreMapGraphData(input);
        expect(resultData.calculatedBacklogs).toEqual(100);

        input = {
            Dates: { Date: '2015-07-31' },
            ScheduledPersonHours: 20,
            BacklogPersonHours: 100,
            PlannedPersonHours: 30
        }

        resultData = target.coreMapGraphData(input);
        expect(resultData.calculatedBacklogs).toEqual(90);

    });

    it('Build graph data seqs should provide seqs with labels', function () {
        target.dictionary = {
            'Backlog': 'Backlog',
            'Scheduled': 'Scheduled',
            'Planned': 'Planned',
            'Underscheduled': 'Underscheduled',
            'Overscheduled': 'Overscheduled',
            'Progress': 'Progress',
            'NeededPersonHours': 'NeededPersonHours',
            'EndDate': 'EndDate',
            'Today': 'Today',
            'Start': 'Start',
            'Backlog ': 'Backlog '
        };

        var input = {
            Dates: [{ Date: '2015-07-31' }],
            ScheduledPersonHours: [50],
            BacklogPersonHours: [100],
            PlannedPersonHours: [30]
        };


        var resultData = target.buildGraphDataSeqs(input);

        ['rawBacklogs', 'calculatedBacklogs', 'plans', 'unscheduledPlans', 'schedules', 'underDiffs', 'overDiffs', 'progress']
            .forEach(function (key) {               
                expect(resultData[key]).toBeDefined();
                expect(Object.keys(target.dictionary)).toContain(resultData[key][0]);
            });
    });


    it('Should build right progress data', function () {
        target.dictionary = {
            'Backlog': 'Backlog',
            'Scheduled': 'Scheduled',
            'Planned': 'Planned',
            'Underscheduled': 'Underscheduled',
            'Overscheduled': 'Overscheduled',
            'Progress': 'Progress',
            'NeededPersonHours': 'NeededPersonHours',
            'EndDate': 'EndDate',
            'Today': 'Today',
            'Start': 'Start',
            'Backlog ': 'Backlog '
        };

        var input = {
            Dates: [{ Date: '2015-07-31' }, { Date: '2015-07-32' }],
            ScheduledPersonHours: [50, 10],
            BacklogPersonHours: [100, 50],
            PlannedPersonHours: [30, 20]
        };


        var result = target.buildGraphDataSeqs(input)['progress'];
        var expectedResult = ['Progress', 150, 100, 50];

        expect(result.length).toEqual(expectedResult.length);

        for (var i = 0; i < expectedResult.length; i++) {
            expect(result[i]).toEqual(expectedResult[i]);
        }

    });

});