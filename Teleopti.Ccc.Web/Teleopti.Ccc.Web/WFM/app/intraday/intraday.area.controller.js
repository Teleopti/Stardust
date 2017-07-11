(function() {
    'use strict';
    angular.module('wfm.intraday').controller('IntradayAreaCtrl', intradayController);

    intradayController.$inject = [
        '$scope',
        '$state',
        'intradayService',
        '$filter',
        'NoticeService',
        '$interval',
        '$timeout',
        '$compile',
        '$translate',
        'intradayTrafficService',
        'intradayPerformanceService',
        'intradayMonitorStaffingService',
        'intradayLatestTimeService',
        'Toggle'
    ];

    function intradayController(
        $scope,
        $state,
        intradayService,
        $filter,
        NoticeService,
        $interval,
        $timeout,
        $compile,
        $translate,
        intradayTrafficService,
        intradayPerformanceService,
        intradayMonitorStaffingService,
        intradayLatestTimeService,
        toggleSvc
    ) {
        var vm = this;
        var autocompleteSkill;
        var autocompleteSkillArea;
        var timeoutPromise;
        var polling;
        var pollingTimeout = 60000;
        $scope.DeleteSkillAreaModal = false;
        $scope.prevArea;
        $scope.drillable;
        $scope.toggles = {
            showOptimalStaffing: [],
            showScheduledStaffing: [],
            showEsl: [],
            showEmailSkill: [],
            showOtherDay: [],
            exportToExcel: []
        };
        var message = $translate
            .instant('WFMReleaseNotificationWithoutOldModuleLink')
            .replace('{0}', $translate.instant('Intraday'))
            .replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
            .replace('{2}', '</a>');
        var prevSkill;
        $scope.currentInterval = [];
        $scope.format = intradayService.formatDateTime;
        $scope.viewObj;
        $scope.chosenOffset = {value: 0};
        NoticeService.info(message, null, true);
        toggleSvc.togglesLoaded.then(function() {
            $scope.toggles.showOptimalStaffing = toggleSvc.Wfm_Intraday_OptimalStaffing_40921;
            $scope.toggles.showScheduledStaffing = toggleSvc.Wfm_Intraday_ScheduledStaffing_41476;
            $scope.toggles.showEsl = toggleSvc.Wfm_Intraday_ESL_41827;
            $scope.toggles.showEmailSkill = toggleSvc.Wfm_Intraday_SupportSkillTypeEmail_44002;
            $scope.toggles.showOtherDay = toggleSvc.WFM_Intraday_Show_For_Other_Days_43504;
            $scope.toggles.exportToExcel = toggleSvc.WFM_Intraday_Export_To_Excel_44892;
        });

        var getAutoCompleteControls = function() {
            var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
            autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

            var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
            autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
        };

        $scope.openSkillFromArea = function(item) {
            if (item.DoDisplayData) {
                prevSkill = item;
                autocompleteSkill.selectedSkill = item;
                $scope.drillable = true;
            } else {
                UnsupportedSkillNotice();
            }
        };

        $scope.openSkillAreaFromSkill = function() {
            autocompleteSkillArea.selectedSkillArea = $scope.prevArea;
            $scope.drillable = false;
        };

        $scope.skillSelected = function(item) {
            $scope.selectedItem = item;
            clearSkillAreaSelection();
        };

        $scope.skillAreaSelected = function(item) {
            $scope.selectedItem = item;
            clearSkillSelection();
        };

        $scope.deleteSkillArea = function(skillArea) {
            cancelTimeout();
            intradayService.deleteSkillArea
                .remove({
                    id: skillArea.Id
                })
                .$promise.then(function(result) {
                    $scope.skillAreas.splice($scope.skillAreas.indexOf(skillArea), 1);
                    $scope.selectedItem = null;
                    $scope.hasMonitorData = false;
                    clearSkillAreaSelection();
                    notifySkillAreaDeletion();
                });

            $scope.toggleModal();
        };

        var clearPrev = function() {
            $scope.drillable = false;
            $scope.prevArea = false;
            prevSkill = false;
        };

        $scope.selectedSkillChange = function(item) {
            if (item) {
                if (item.DoDisplayData) {
					// $scope.chosenOffset.value = 0;
					$scope.changeChosenOffset($scope.chosenOffset.value);
                    $scope.skillSelected(item);
                    pollActiveTabDataByDayOffset($scope.activeTab, $scope.chosenOffset.value);
                    if (prevSkill) {
                        if (!(prevSkill === autocompleteSkill.selectedSkill)) {
                            clearPrev();
                        }
                        return;
                    }
                } else {
                    clearSkillSelection();
                    UnsupportedSkillNotice();
                }
            }
        };

        function UnsupportedSkillNotice() {
            var notPhoneMessage = $translate.instant('UnsupportedSkillMsg');
            NoticeService.warning(notPhoneMessage, 5000, true);
        }

        $scope.selectedSkillAreaChange = function(item) {
            if (item) {
                // $scope.chosenOffset.value = 0;
                $scope.skillAreaSelected(item);

                pollData($scope.activeTab);

                $scope.prevArea = $scope.selectedItem;
                item.UnsupportedSkills = [];
                checkUnsupported(item);
            }
            if ($scope.drillable === true && $scope.selectedItem.skills) {
                $scope.drillable = false;
            }
        };

        function isSupported(skill) {
            return skill.DoDisplayData === true;
        }

        $scope.setDynamicIcon = function(skill) {
            if (!skill.DoDisplayData) {
                return 'mdi mdi-alert';
            }

            if (skill.IsMultisiteSkill) {
                return 'mdi mdi-hexagon-multiple';
            }
            if (skill.SkillType === 'SkillTypeChat') {
                return 'mdi mdi-message-text-outline';
            } else if (skill.SkillType === 'SkillTypeEmail') {
                return 'mdi mdi-email-outline';
            } else if (skill.SkillType === 'SkillTypeEmail') {
                return 'mdi mdi-email-outline';
            } else if (skill.SkillType === 'SkillTypeInboundTelephony') {
                return 'mdi mdi-phone';
            } else if (skill.SkillType === 'SkillTypeRetail') {
                return 'mdi mdi-credit-card';
            }
        };

        var reloadSkillAreas = function(isNew) {
            intradayService.getSkillAreas.query().$promise.then(function(result) {
                getAutoCompleteControls();
                $scope.skillAreas = $filter('orderBy')(result.SkillAreas, 'Name');
                if (isNew) {
                    $scope.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
                    $scope.latest = $filter('orderBy')(result.SkillAreas, 'Name');
                    //TO DO: get a date to filter by
                    // $scope.latest = $filter('orderBy')(result.SkillAreas, 'created_at', true);
                }
                $scope.HasPermissionToModifySkillArea = result.HasPermissionToModifySkillArea;

                intradayService.getSkills.query().$promise.then(function(result) {
                    $scope.skills = result;
                    if ($scope.skillAreas.length === 0) {
                        $scope.selectedItem = $scope.selectedItem = $scope.skills.find(isSupported);
                        if (autocompleteSkill) {
                            autocompleteSkill.selectedSkill = $scope.selectedItem;
                        }
                    }
                    if ($scope.skillAreas.length > 0) {
                        if (isNew) {
                            $scope.selectedItem = $scope.latest[0];
                            if (autocompleteSkillArea) autocompleteSkillArea.selectedSkillArea = $scope.selectedItem;
                        } else {
                            $scope.selectedItem = $scope.skillAreas[0];
                            if (autocompleteSkillArea) autocompleteSkillArea.selectedSkillArea = $scope.selectedItem;
                        }
                    }
                });
            });
        };

        var checkUnsupported = function(item) {
            for (var i = 0; i < item.Skills.length; i++) {
                for (var j = 0; j < $scope.skills.length; j++) {
                    if (item.Skills[i].Id === $scope.skills[j].Id && $scope.skills[j].DoDisplayData === false) {
                        item.UnsupportedSkills.push($scope.skills[j]);
                        item.Skills[i].DoDisplayData = false;
                    } else if (item.Skills[i].Id === $scope.skills[j].Id && $scope.skills[j].DoDisplayData === true) {
                        item.Skills[i].DoDisplayData = true;
                    }
                }
            }
            if (item.UnsupportedSkills.length > 0) {
                $scope.skillAreaMessage = $translate
                    .instant('UnsupportedSkills')
                    .replace('{0}', item.UnsupportedSkills.length);
            } else {
                $scope.skillAreaMessage = '';
            }
        };

        $scope.clearSkillHelper = function() {
            clearSkillSelection();
        };

        $scope.clearSkillAreaHelper = function() {
            clearSkillAreaSelection();
        };

        function clearSkillSelection() {
            if (!autocompleteSkill) return;
            autocompleteSkill.selectedSkill = null;
            autocompleteSkill.searchSkillText = '';
            $scope.drillable = false;
        }

        function clearSkillAreaSelection() {
            if (!autocompleteSkillArea) return;
            autocompleteSkillArea.selectedSkillArea = null;
            autocompleteSkillArea.searchSkillAreaText = '';
        }

        $scope.querySearch = function(query, myArray) {
            var results = query ? myArray.filter(createFilterFor(query)) : myArray,
                deferred;
            return results;
        };

        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(item) {
                var lowercaseName = angular.lowercase(item.Name);
                return lowercaseName.indexOf(lowercaseQuery) === 0;
            };
        }

        if (!$scope.selectedSkillArea && !$scope.selectedSkill && $scope.latestActualInterval === '--:--') {
            $scope.hasMonitorData = false;
        }

        var cancelTimeout = function() {
            if (timeoutPromise) {
                $timeout.cancel(timeoutPromise);
                timeoutPromise = undefined;
            }
        };

        function poll() {
            polling = $interval(function() {
                pollData($scope.activeTab);
            }, pollingTimeout);
        }
        poll();

        $scope.pollActiveTabDataHelper = function(activeTab) {
            $interval.cancel(polling);
            pollData(activeTab);
            if ($scope.chosenOffset.value === 0) {
                poll();
            }
        };
        function pollData(activeTab) {
            if ($scope.toggles.showOtherDay) {
                pollActiveTabDataByDayOffset(activeTab, $scope.chosenOffset.value);
            } else {
                pollActiveTabData(activeTab);
            }
        }

        function pollActiveTabData(activeTab) {
            var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];

            if ($scope.selectedItem !== null && $scope.selectedItem !== undefined) {
                if ($scope.selectedItem.Skills) {
                    services[activeTab].pollSkillAreaData($scope.selectedItem, $scope.toggles);
                    var timeData = intradayLatestTimeService.getLatestTime($scope.selectedItem);
                } else {
                    services[activeTab].pollSkillData($scope.selectedItem, $scope.toggles);
                    var timeData = intradayLatestTimeService.getLatestTime($scope.selectedItem);
                }
                $scope.viewObj = services[activeTab].getData();
                $scope.latestActualInterval = timeData;
                $scope.hasMonitorData = $scope.viewObj.hasMonitorData;
            } else {
                $timeout(function() {
                    pollActiveTabData($scope.activeTab);
                }, 1000);
            }
        }

        function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
            var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
			
            if ($scope.selectedItem !== null && $scope.selectedItem !== undefined) {
                if ($scope.selectedItem.Skills) {
                    services[activeTab].pollSkillAreaDataByDayOffset($scope.selectedItem, $scope.toggles, dayOffset);
                    if (dayOffset === 0) {
                        var timeData = intradayLatestTimeService.getLatestTime($scope.selectedItem);
                    }
                } else {
                    services[activeTab].pollSkillDataByDayOffset($scope.selectedItem, $scope.toggles, dayOffset);
                    if (dayOffset === 0) {
                        var timeData = intradayLatestTimeService.getLatestTime($scope.selectedItem);
                    }
                }
                $scope.viewObj = services[activeTab].getData();
                $scope.latestActualInterval = timeData;
                $scope.hasMonitorData = $scope.viewObj.hasMonitorData;
            } else {
                $timeout(function() {
                    pollActiveTabDataByDayOffset($scope.activeTab, dayOffset);
                }, 1000);
            }
        }

        $scope.$on('$destroy', function(event) {
            cancelTimeout();
        });

        $scope.$on('$locationChangeStart', function() {
            cancelTimeout();
        });

        $scope.configMode = function() {
            $state.go('intraday.config', {
                isNewSkillArea: false
            });
        };

        $scope.toggleModal = function() {
            $scope.DeleteSkillAreaModal = !$scope.DeleteSkillAreaModal;
        };

        $scope.onStateChanged = function(evt, to, params, from) {
            if (to.name !== 'intraday.area') return;
            if (params.isNewSkillArea === true) {
                reloadSkillAreas(true);
            } else reloadSkillAreas(false);
        };

        $scope.$on('$stateChangeSuccess', $scope.onStateChanged);

        $scope.$on('$viewContentLoaded', function() {
            pollActiveTabData($scope.activeTab);
        });

        var notifySkillAreaDeletion = function() {
            var message = $translate.instant('Deleted');
            NoticeService.success(message, 5000, true);
        };

        $scope.$on('$destroy', function() {
            $interval.cancel(polling);
        });

        $scope.getLocalDate = function(offset) {
            var ret = moment().add(offset, 'days').format('dddd, LL');
            return ret;
        };

        $scope.getUTCDate = function(offset) {
            var ret = moment.utc().toISOString();

            if ($scope.toggles.showOtherDay) {
                ret = moment.utc().add(offset, 'days').toISOString();
            }

            return ret;
        };

        $scope.changeChosenOffset = function(value) {
            $interval.cancel(polling);
            var d = angular.copy($scope.chosenOffset);
            d.value = value;
            $scope.chosenOffset = d;
            pollActiveTabDataByDayOffset($scope.activeTab, value);
            if (value === 0) {
                poll();
			}
			
        };
    }
})();
