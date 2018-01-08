(function() {
    'use strict';
    angular.module('wfm.intraday').controller('IntradayAreaController', intradayController);

    intradayController.$inject = [
        '$scope',
        '$state',
        'intradayService',
        'SkillGroupSvc',
        '$filter',
        'NoticeService',
        '$interval',
        '$timeout',
        '$translate',
        'intradayTrafficService',
        'intradayPerformanceService',
        'intradayMonitorStaffingService',
        'intradayLatestTimeService',
        'Toggle',
        'skillIconService',
        'CurrentUserInfo'
    ];

    function intradayController(
        $scope,
        $state,
        intradayService,
        SkillGroupSvc,
        $filter,
        NoticeService,
        $interval,
        $timeout,
        $translate,
        intradayTrafficService,
        intradayPerformanceService,
        intradayMonitorStaffingService,
        intradayLatestTimeService,
        toggleSvc,
        skillIconService,
        currentUserInfo
    ) {
        var vm = this;
        var autocompleteSkill;
        var autocompleteSkillArea;
        var timeoutPromise;
        var polling;
        var pollingTimeout = 60000;
        var message = $translate
            .instant('WFMReleaseNotificationWithoutOldModuleLink')
            .replace('{0}', $translate.instant('Intraday'))
            .replace('{1}', "<a href=' http://www.teleopti.com/wfm/customer-feedback.aspx' target='_blank'>")
            .replace('{2}', '</a>');
        var prevSkill;

        vm.DeleteSkillAreaModal = false;
        vm.activeTab = 0;
        vm.getSkillIcon = skillIconService.get;
        vm.toggles = {
            showOptimalStaffing: [],
            showScheduledStaffing: [],
            showEsl: [],
            showEmailSkill: [],
            showOtherDay: [],
            exportToExcel: [],
            otherSkillsLikeEmail: [],
            unifiedSkillGroupManagement: [],
            modifySkillGroups: []
        };
        vm.currentInterval = [];
        vm.format = intradayService.formatDateTime;
        vm.viewObj;
        vm.chosenOffset = {
            value: 0
        };
        vm.filterSkills = [];

        vm.changeChosenOffset = function(value, dontPoll) {
            $interval.cancel(polling);
            cancelTimeout();
            var d = angular.copy(vm.chosenOffset);
            d.value = value;
            vm.chosenOffset = d;
            if (!dontPoll) {
                pollActiveTabDataByDayOffset(vm.activeTab, value);
            }
            if (value === 0) {
                poll();
            }
        };

        vm.checkIfFilterSkill = function(skill) {
            if (vm.filterSkills.indexOf(skill) !== -1) {
                return 'mdi mdi-check';
            }
            return vm.getSkillIcon(skill);
        };

        vm.chipClass = function(skill) {
            if (!skill.DoDisplayData) {
                return 'chip-warning';
            }

            if (vm.filterSkills.indexOf(skill) !== -1) {
                return 'chip-success';
            }

            return;
        };

        vm.clearSkillHelper = function() {
            clearSkillSelection();
        };

        vm.clearSkillAreaHelper = function() {
            clearSkillAreaSelection();
        };

        vm.configMode = function() {
            $state.go('intraday.skill-area-config', {
                isNewSkillArea: false
            });
        };

        vm.deleteSkillArea = function(skillArea) {
            cancelTimeout();
            SkillGroupSvc.deleteSkillGroup({
                Id: skillArea.Id
            }).then(function() {
                vm.skillAreas.splice(vm.skillAreas.indexOf(skillArea), 1);
                vm.selectedItem = null;
                vm.hasMonitorData = false;
                clearSkillAreaSelection();
                notifySkillAreaDeletion();
            });

            vm.toggleModal();
        };

        vm.exportIntradayData = function() {
            if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem) && !vm.exporting) {
                vm.exporting = true;

                if (vm.selectedItem.Skills) {
                    intradayService.getIntradayExportForSkillArea(
                        angular.toJson({
                            id: vm.selectedItem.Id,
                            dayOffset: vm.chosenOffset.value
                        }),
                        saveData,
                        errorSaveData
                    );
                } else {
                    intradayService.getIntradayExportForSkill(
                        angular.toJson({
                            id: vm.selectedItem.Id,
                            dayOffset: vm.chosenOffset.value
                        }),
                        saveData,
                        errorSaveData
                    );
                }
            }
        };

        vm.getLocalDate = function (offset) {
            var userDate = getUserDateTime();
            var offset = userDate
                .add(offset, 'days')
                .format('dddd, LL');

            return offset.charAt(0).toUpperCase() + offset.substr(1).toLowerCase();
        };

        vm.getSkillGroupText = function() {
            return vm.toggles.unifiedSkillGroupManagement
                ? $translate.instant('SelectSkillGroup')
                : $translate.instant('SelectSkillArea');
        };

        vm.getUTCDate = function(offset) {
            var ret = moment.utc().toISOString();

            if (vm.toggles.showOtherDay) {
                ret = moment
                    .utc()
                    .add(offset, 'days')
                    .toISOString();
            }

            return ret;
        };

        vm.onStateChanged = function(evt, to, params, from) {
            if (to.name !== 'intraday.area') return;
            if (params.isNewSkillArea === true) {
                reloadSkillAreas(true);
            } else reloadSkillAreas(false);
        };

        vm.openSkillFromArea = function(skill) {
            if (skill.DoDisplayData) {
                var skillIndex = vm.filterSkills.indexOf(skill);
                if (skillIndex === -1) {
                    vm.filterSkills = [];
                    vm.filterSkills.push(skill);
                    vm.selectedItem = skill;
                    vm.selectedSkillInAreaChange(skill);
                } else {
                    vm.selectedSkillAreaChange(vm.selectedSkillArea);
                    vm.filterSkills.splice(skillIndex, 1);
                }
            } else {
                UnsupportedSkillNotice();
            }
        };

        vm.openSkillGroupManager = function() {
            $state.go('intraday.skill-area-manager', {
                isNewSkillArea: false,
                selectedGroup: vm.selectedSkillArea
            });
        };

        vm.pollActiveTabDataHelper = function(activeTab) {
            pollData(activeTab);
            if (vm.chosenOffset.value === 0) {
                poll();
            }
        };

        vm.querySearch = function(query, myArray) {
            var results = query ? myArray.filter(createFilterFor(query)) : myArray,
                deferred;
            return results;
        };

        vm.selectedSkillAreaChange = function(item) {
            if (item) {
                vm.filterSkills = [];
                vm.skillAreaSelected(item);
                pollData(vm.activeTab);

                vm.prevArea = vm.selectedItem;
                item.UnsupportedSkills = [];
                checkUnsupported(item);
            } else if (vm.selectedSkill === null) {
                vm.selectedItem = null;
            }
        };

        vm.selectedSkillInAreaChange = function(skill) {
            if (skill) {
                if (skill.DoDisplayData) {
                    pollActiveTabDataByDayOffset(vm.activeTab, vm.chosenOffset.value);
                } else {
                    vm.selectedItem = vm.selectedSkillArea;
                    UnsupportedSkillNotice();
                }
            }
        };

        vm.selectedSkillChange = function(item) {
            if (item) {
                vm.filterSkills = [];
                if (item.DoDisplayData) {
                    vm.skillSelected(item);
                    pollActiveTabDataByDayOffset(vm.activeTab, vm.chosenOffset.value);
                    if (prevSkill) {
                        if (!(prevSkill === vm.selectedSkill)) {
                            clearPrev();
                        }
                    }
                } else {
                    clearSkillSelection();
                    UnsupportedSkillNotice();
                }
            } else if (vm.selectedSkillArea === null) {
                vm.selectedItem = null;
            }
        };

        vm.skillAreaSelected = function(item) {
            vm.selectedItem = item;
            vm.selectedSkillArea = item;
            clearSkillSelection();
        };

        vm.skillSelected = function(item) {
            clearSkillAreaSelection();
            vm.selectedItem = vm.selectedSkill = item;
        };

        vm.toggleModal = function() {
            vm.DeleteSkillAreaModal = !vm.DeleteSkillAreaModal;
        };

        function cancelTimeout() {
            if (timeoutPromise) {
                $timeout.cancel(timeoutPromise);
                timeoutPromise = undefined;
            }
        }

        function checkUnsupported(item) {
            for (var i = 0; i < item.Skills.length; i++) {
                for (var j = 0; j < vm.skills.length; j++) {
                    if (item.Skills[i].Id === vm.skills[j].Id && vm.skills[j].DoDisplayData === false) {
                        item.UnsupportedSkills.push(vm.skills[j]);
                        item.Skills[i].DoDisplayData = false;
                    } else if (item.Skills[i].Id === vm.skills[j].Id && vm.skills[j].DoDisplayData === true) {
                        item.Skills[i].DoDisplayData = true;
                    }
                }
            }
            if (item.UnsupportedSkills.length > 0) {
                vm.skillAreaMessage = $translate
                    .instant('UnsupportedSkills')
                    .replace('{0}', item.UnsupportedSkills.length);
            } else {
                vm.skillAreaMessage = '';
            }
        }

        function clearPrev() {
            vm.prevArea = false;
            prevSkill = false;
        }

        function clearSkillAreaSelection() {
            if (!autocompleteSkillArea) return;
            vm.selectedSkillArea = null;
            vm.searchSkillAreaText = '';
        }

        function clearSkillSelection() {
            if (!autocompleteSkill) return;
            vm.selectedSkill = null;
            vm.searchSkillText = '';
        }

        function createFilterFor(query) {
            var lowercaseQuery = angular.lowercase(query);
            return function filterFn(item) {
                var lowercaseName = angular.lowercase(item.Name);
                return lowercaseName.indexOf(lowercaseQuery) === 0;
            };
        }

        function errorSaveData(data, status, headers, config) {
            vm.exporting = false;
        }

        function getAutoCompleteControls() {
            var autocompleteSkillDOM = document.querySelector('.autocomplete-skill');
            autocompleteSkill = angular.element(autocompleteSkillDOM).scope();

            var autocompleteSkillAreaDOM = document.querySelector('.autocomplete-skillarea');
            autocompleteSkillArea = angular.element(autocompleteSkillAreaDOM).scope();
        }

        function isSupported(skill) {
            return skill.DoDisplayData === true;
        }

        function notifySkillAreaDeletion() {
            var message = $translate.instant('Deleted');
            NoticeService.success(message, 5000, true);
        }

        function poll() {
            $interval.cancel(polling);
            polling = $interval(function() {
                pollData(vm.activeTab);
            }, pollingTimeout);
        }

        function pollActiveTabData(activeTab) {
            var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
            if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem)) {
                if (vm.selectedItem.Skills) {
                    services[activeTab].pollSkillAreaData(vm.selectedItem, vm.toggles);
                    var timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
                } else {
                    services[activeTab].pollSkillData(vm.selectedItem, vm.toggles);
                    var timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
                }
                vm.viewObj = services[activeTab].getData();
                vm.latestActualInterval = timeData;
                vm.hasMonitorData = vm.viewObj.hasMonitorData;
            } else {
                timeoutPromise = $timeout(function() {
                    pollActiveTabData(vm.activeTab);
                }, 1000);
            }
        }

        function pollActiveTabDataByDayOffset(activeTab, dayOffset) {
            var services = [intradayTrafficService, intradayPerformanceService, intradayMonitorStaffingService];
            var timeData;
            if (vm.selectedItem !== null && angular.isDefined(vm.selectedItem)) {
                if (vm.selectedItem.Skills) {
                    services[activeTab].pollSkillAreaDataByDayOffset(vm.selectedItem, vm.toggles, dayOffset);
                    if (dayOffset === 0) {
                        timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
                    }
                } else {
                    services[activeTab].pollSkillDataByDayOffset(vm.selectedItem, vm.toggles, dayOffset);
                    if (dayOffset === 0) {
                        timeData = intradayLatestTimeService.getLatestTime(vm.selectedItem);
                    }
                }
                vm.viewObj = services[activeTab].getData();
                vm.latestActualInterval = timeData;
                vm.hasMonitorData = vm.viewObj.hasMonitorData;
            } else {
                timeoutPromise = $timeout(function() {
                    pollActiveTabDataByDayOffset(vm.activeTab, dayOffset);
                }, 1000);
            }
        }

        function pollData(activeTab) {
            if (vm.toggles.showOtherDay) {
                pollActiveTabDataByDayOffset(activeTab, vm.chosenOffset.value);
            } else {
                pollActiveTabData(activeTab);
            }
        }

        function reloadSkillAreas(isNew) {
            SkillGroupSvc.getSkillGroups().then(function(result) {
                getAutoCompleteControls();
                vm.skillAreas = $filter('orderBy')(result.data.SkillAreas, 'Name');
                if (isNew) {
                    vm.latest = $filter('orderBy')(result.data.SkillAreas, 'created_at', true);
                    vm.latest = $filter('orderBy')(result.data.SkillAreas, 'Name');
                }
                vm.HasPermissionToModifySkillArea = result.data.HasPermissionToModifySkillArea;

                SkillGroupSvc.getSkills().then(function(result) {
                    vm.skills = result.data;
                    if (vm.skillAreas.length === 0) {
                        vm.selectedItem = vm.skills.find(isSupported);
                        if (autocompleteSkill) {
                            vm.selectedSkill = vm.selectedItem;
                        }
                    }
                    if (vm.skillAreas.length > 0) {
                        if (isNew) {
                            vm.selectedItem = vm.latest[0];
                            if (autocompleteSkillArea) vm.selectedSkillArea = vm.selectedItem;
                        } else {
                            if (vm.toggles.modifySkillGroups) {
                                vm.selectedItem = null;
                            } else {
                                vm.selectedItem = vm.skillAreas[0];
                            }
                            if (autocompleteSkillArea) vm.selectedSkillArea = vm.selectedItem;
                        }
                    }
                });
            });
        }

        function saveData(data, status, headers, config) {
            var blob = new Blob([data]);
            vm.exporting = false;
            saveAs(blob, 'IntradayExportedData ' + moment().format('YYYY-MM-DD') + '.xlsx');
        }
        function UnsupportedSkillNotice() {
            var notPhoneMessage = $translate.instant('UnsupportedSkillMsg');
            NoticeService.warning(notPhoneMessage, 5000, true);
		}

        function getUserDateTime() {
            return moment.tz(moment(), currentUserInfo.CurrentUserInfo().DefaultTimeZone);
        }


        $scope.$on('$destroy', function(event) {
            cancelTimeout();
        });

        $scope.$on('$locationChangeStart', function() {
            cancelTimeout();
        });

        $scope.$on('$stateChangeSuccess', vm.onStateChanged);

        $scope.$on('$viewContentLoaded', function() {
            pollData(vm.activeTab);
        });

        $scope.$on('$destroy', function() {
            $interval.cancel(polling);
        });

        toggleSvc.togglesLoaded.then(function() {
            vm.toggles.showOptimalStaffing = toggleSvc.Wfm_Intraday_OptimalStaffing_40921;
            vm.toggles.showScheduledStaffing = toggleSvc.Wfm_Intraday_ScheduledStaffing_41476;
            vm.toggles.showEsl = toggleSvc.Wfm_Intraday_ESL_41827;
            vm.toggles.showEmailSkill = toggleSvc.Wfm_Intraday_SupportSkillTypeEmail_44002;
            vm.toggles.showOtherDay = toggleSvc.WFM_Intraday_Show_For_Other_Days_43504;
            vm.toggles.exportToExcel = toggleSvc.WFM_Intraday_Export_To_Excel_44892;
            vm.toggles.otherSkillsLikeEmail = toggleSvc.WFM_Intraday_SupportOtherSkillsLikeEmail_44026;
            vm.toggles.unifiedSkillGroupManagement = toggleSvc.WFM_Unified_Skill_Group_Management_45417;
            vm.toggles.modifySkillGroups = toggleSvc.WFM_Modify_Skill_Groups_45727;
        });

        if (vm.latestActualInterval === '--:--') {
            vm.hasMonitorData = false;
        }

        NoticeService.info(message, null, true);

        poll();
    }
})();
