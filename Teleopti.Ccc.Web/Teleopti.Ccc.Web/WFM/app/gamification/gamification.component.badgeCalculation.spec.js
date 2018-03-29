describe('badgeCalculation', function () {
    var attachedElements = [];
    var $compile,
        $rootScope,
        $material,
        $q,
        $document,
        $httpBackend;


    beforeEach(function () {
        module('wfm.templates', 'externalModules', 'wfm.gamification', 'ngMaterial', 'ngMaterial-mock');

        module(function ($provide) {
            $provide.service('GamificationDataService', function () { return new DataService(); });
        });

        inject(function ($injector) {
            $compile = $injector.get('$compile');
            $rootScope = $injector.get('$rootScope');
            $material = $injector.get('$material');
            $document = $injector.get('$document');
            $httpBackend = $injector.get('$httpBackend');
            $q = $injector.get('$q');

            $httpBackend.when('GET', '../ToggleHandler/AllToggles').respond('');
        });
    });

    afterEach(function () {
        attachedElements.forEach(function (element) {
            var elementScope = element.scope();

            elementScope && elementScope.$destroy();
            element.remove();
        });
        attachedElements = [];
    });

    it('should show error if the date range has intersection with one of the running job.', function () {
        var cmp = setupComponent(testScope());
        var ctrl = cmp.controller('badgeCalculation');
        expect(ctrl).toBeTruthy();
        ctrl.dateRange = {
            startDate: 'Mar 1, 2018',
            endDate: 'Mar 6, 2018'
        };

        ctrl.jobs = [{
            id: 'job1',
            owner: 'demo',
            startDate: 'Mar 5, 2018',
            endDate: 'Mar 10, 2018',
            status: 'inprogress'
        }];

        expect(ctrl.hasIntersection()).toBe(true);
    });

    it('should not show error if the date range has no intersection with all of the running job.', function () {
        var cmp = setupComponent(testScope());
        var ctrl = cmp.controller('badgeCalculation');
        expect(ctrl).toBeTruthy();
        ctrl.dateRange = {
            startDate: 'Mar 1, 2018',
            endDate: 'Mar 6, 2018'
        };

        ctrl.jobs = [{
            id: 'job1',
            owner: 'demo',
            startDate: 'Mar 7, 2018',
            endDate: 'Mar 10, 2018',
            status: 'inprogress'
        }];

        expect(ctrl.hasIntersection()).toBe(false);
    });

    it('should render badge calculation page', function () {
        var cmp = setupComponent(testScope());
        var startBtn = cmp[0].querySelector('button.wfm-btn.ng-binding');
        expect(startBtn).not.toBe(null);
    });

    function setupComponent(scope) {
        var el;

        var template = '<badge-calculation></badge-calculation>';

        el = $compile(template)(scope || $rootScope);

        if (scope) {
            scope.$apply();
        } else {
            $rootScope.$digest();
        }

        attachedElements.push(el);
        $document[0].body.appendChild(el[0]);
        return el;
    }

    function testScope() {
        var scope = $rootScope.$new();
        return scope;
    }

    function DataService() {
        this.fetchCalculationJobs = function () {
            return $q(function (resolve, reject) {
                resolve([
                    { id: 0, owner: 'demo0' },
                    { id: 1, owner: 'demo0' },
                ]);
            });
        };

        this.fetchPurgeDays = function () {
            return $q(function (resolve, reject) {
                resolve(30);
            });
        }
    }
}); 