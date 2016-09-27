(function () {
    'use strict';
    describe('Action Notice Service', function () {
        var actionNoticeService;
        var $rootScope;
        var $timeout;

        beforeEach(function () {
            module('wfm.teamSchedule');
            removeAllNotices();
        });

        beforeEach(inject(['actionNoticeService', '$rootScope', '$timeout', function (_actionNoticeService_, _$rootScope_, _$timeout_) {
            actionNoticeService = _actionNoticeService_;
            $rootScope = _$rootScope_;
            $timeout = _$timeout_;
        }]));

        it('should display a notice', function () {
            var noticeOptions = {
                type: 'success',
                icon: 'success',
                content: 'a success message',
                timeToLive: null
            };
            actionNoticeService.newNotice(noticeOptions);
            expect(document.querySelectorAll('ts-action-notice').length).toBe(1);
        });

        it('should include custom actions', function () {
            var noticeOptions = {
                type: 'success',
                icon: 'success',
                content: 'a success message',
                template: '<button class="custom-action">Action</button>',
                timeToLive: null
            };
            actionNoticeService.newNotice(noticeOptions);
            expect(document.querySelectorAll('ts-action-notice').length).toBe(1);
            expect(document.querySelector('ts-action-notice').querySelectorAll('.custom-action').length).toBe(1);
        });

        it('should bind custom controller', function () {
            function myController() { }
            var noticeOptions = {
                type: 'success',
                icon: 'success',
                content: 'a success message',
                template: '<button class="custom-action">Action</button>',
                controller: myController,
                timeToLive: null
            };
            actionNoticeService.newNotice(noticeOptions);
            var theNotice = document.querySelector('ts-action-notice');
            var theController = angular.element(theNotice).controller();
            expect(theController.constructor).toBe(myController);
        });

        it('should remove the notice after timeout', function (done) {
            function myController() { }
            var noticeOptions = {
                type: 'success',
                icon: 'success',
                content: 'a success message',
                template: '<button class="custom-action">Action</button>',
                controller: myController,
                timeToLive: 1000
            };
            actionNoticeService.newNotice(noticeOptions);
            $timeout(function () {
                expect(document.querySelectorAll('ts-action-notice').length).toBe(0);
                done();
            }, 2000);
            $timeout.flush();
        });

        it('should remove the notice upon state change', function () {
            var noticeOptions = {
                type: 'success',
                icon: 'success',
                content: 'a success message',
                destroyOnStateChange: true
            };
            actionNoticeService.newNotice(noticeOptions);
            expect(document.querySelectorAll('ts-action-notice').length).toBe(1);

            $rootScope.$broadcast('$stateChangeSuccess');
            expect(document.querySelectorAll('ts-action-notice').length).toBe(0);
        });

        function removeAllNotices() {
            var notices = document.querySelectorAll('ts-action-notice');
            angular.forEach(notices, function (notice) {
                notice.parentNode.removeChild(notice);
            });
        }
    });
})();
