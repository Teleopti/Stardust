fdescribe('badgeCalculation', function () {
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

    fit('should render badge calculation page', function () {
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
    }
}); 