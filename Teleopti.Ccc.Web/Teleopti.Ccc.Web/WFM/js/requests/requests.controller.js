(function () {
	'use strict';

	angular.module('wfm.requests').controller('RequestsCtrl', requestsController)
        .run(['$templateCache', overrideTemplateForMultiSelect]);

	requestsController.$inject = ["$scope", "Toggle", "requestsDefinitions", "requestsNotificationService", "CurrentUserInfo"];

	function overrideTemplateForMultiSelect($templateCache) {
	    var template = '<span class="multiSelect inlineBlock">'
			+ '	<button id="{{directiveId}}" type="button" class="wfm-btn wfm-btn-default"'
			+ '		ng-click="toggleCheckboxes( $event ); refreshSelectedItems(); refreshButton(); prepareGrouping; prepareIndex();"'
			+ '		ng-bind-html="varButtonLabel" ng-disabled="disable-button">'
			+ '	</button>'
			+ '	<div class="checkboxLayer">'
			+ '		<div class="helperContainer" ng-if="helperStatus.filter || helperStatus.all || helperStatus.none || helperStatus.reset">'
			+ '			<div class="line" ng-if="helperStatus.all || helperStatus.none || helperStatus.reset">'
			+ '				<button type="button" class="wfm-btn wfm-btn-invis-default helperButton"'
			+ '					ng-disabled="isDisabled"'
			+ '					ng-if="helperStatus.all"'
			+ '					ng-click="select( \'all\', $event );"'
			+ '					ng-bind-html="lang.selectAll">'
			+ '				</button>'
			+ '				<button type="button" class="wfm-btn wfm-btn-invis-default helperButton"'
			+ '					ng-disabled="isDisabled"'
			+ '					ng-if="helperStatus.none"'
			+ '					ng-click="select( \'none\', $event );"'
			+ '					ng-bind-html="lang.selectNone">'
			+ '				</button>'
			+ '				<button type="button" class="wfm-btn wfm-btn-invis-default helperButton reset"'
			+ '					ng-disabled="isDisabled"'
			+ '					ng-if="helperStatus.reset"'
			+ '					ng-click="select( \'reset\', $event );"'
			+ '					ng-bind-html="lang.reset">'
			+ '				</button>'
			+ '			</div>'
			+ '			<div class="line" style="position:relative" ng-if="helperStatus.filter">'
			+ '				<input placeholder="{{lang.search}} 1234" type="text"'
			+ '					ng-click="select( \'filter\', $event )"'
			+ '					ng-model="inputLabel.labelFilter"'
			+ '					ng-change="searchChanged()" class="inputFilter" />'
			+ '				<button type="button" class="wfm-btn wfm-btn-invis-default clearButton" ng-click="clearClicked( $event )">×</button>'
			+ '			</div>'
			+ '		</div>'
			+ '		<ul class="checkBoxContainer">'
			+ '			<li class="multiSelectItem" ng-repeat="item in filteredModel | filter:removeGroupEndMarker"'
			+ '				ng-class="{selected: item[ tickProperty ], horizontal: orientationH, vertical: orientationV, multiSelectGroup:item[ groupProperty ], disabled:itemIsDisabled( item )}"'
			+ '				ng-click="syncItems( item, $event, $index );" ng-mouseleave="removeFocusStyle( tabIndex );" tabindex="0" role="button" >'
			+ '				<div class="acol" ng-if="item[ spacingProperty ] > 0" ng-repeat="i in numberToArray( item[ spacingProperty ] ) track by $index"></div>'
			+ '				<div class="acol">' 
			+ '					<label>'
			+ '						<input class="checkbox focusable" type="checkbox"'
			+ '							ng-disabled="itemIsDisabled( item )"'
			+ '							ng-checked="item[ tickProperty ]"'
			+ '							ng-click="syncItems( item, $event, $index )" />'
			+ '						<span aria-hidden="false" ng-show="item[ groupProperty ] !== true && item[ tickProperty ] === true" class="list-dot grow-out">'
			+ '							<b class="mdi mdi-check list-mark"></b>'
			+ '						</span>'
			+ '						<span class="list-item-container" ng-class="{disabled:itemIsDisabled( item )}" ng-bind-html="writeLabel( item, \'itemLabel\' )"></span>'
			+ '					</label>'
			+ '				</div>'
			+ '			</li>'
			+ '		</ul>'
			+ '	</div>'
			+ '</span>';
		$templateCache.put("isteven-multi-select.htm", template);
	}

    function requestsController($scope, toggleService, requestsDefinitions, requestsNotificationService, CurrentUserInfo) {
        var vm = this;
        vm.onAgentSearchTermChanged = onAgentSearchTermChanged;

        toggleService.togglesLoaded.then(init);
		
        function init() {
            monitorRunRequestWaitlist();
            vm.isRequestsEnabled = toggleService.Wfm_Requests_Basic_35986;
            vm.isPeopleSearchEnabled = toggleService.Wfm_Requests_People_Search_36294;
            vm.isShiftTradeViewActive = isShiftTradeViewActive;
            vm.isRequestsCommandsEnabled = toggleService.Wfm_Requests_ApproveDeny_36297;
			vm.isShiftTradeViewVisible = toggleService.Wfm_Requests_ShiftTrade_37751;
            vm.forceRequestsReloadWithoutSelection = forceRequestsReloadWithoutSelection;
		
			vm.dateRangeTemplateType = 'popup';
			
			vm.filterToggleEnabled = toggleService.Wfm_Requests_Filtering_37748;
			vm.filterEnabled = vm.filterToggleEnabled;
			
            vm.period = { startDate: new Date(), endDate: new Date() };

            vm.agentSearchOptions = {
                keyword: "",
                isAdvancedSearchEnabled: true,
                searchKeywordChanged: false
            };
            vm.agentSearchTerm = vm.agentSearchOptions.keyword;

            vm.onBeforeCommand = onBeforeCommand;
            vm.onCommandSuccess = onCommandSuccess;
            vm.onCommandError = onCommandError;
            vm.onErrorMessages = onErrorMessages;
            vm.disableInteraction = false;

        }

		function isShiftTradeViewActive() {
			return vm.selectedTabIndex === 1;
        }
		
        function onAgentSearchTermChanged(agentSearchTerm) {
            vm.agentSearchTerm = agentSearchTerm;
        }

        function forceRequestsReloadWithoutSelection() {
        	$scope.$broadcast('reload.requests.without.selection');
        }

        function onBeforeCommand() {
            vm.disableInteraction = true;
            return true;
        }

        function onCommandSuccess(commandType, changedRequestsCount, requestsCount, commandId, waitlistPeriod) {
            vm.disableInteraction = false;
            forceRequestsReloadWithoutSelection();
            if (commandId) vm.commandIdForMessage = commandId;
            if (commandType === requestsDefinitions.REQUEST_COMMANDS.Approve) {
                requestsNotificationService.notifyApproveRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Deny) {
                requestsNotificationService.notifyDenyRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.Cancel) {
                requestsNotificationService.notifyCancelledRequestsSuccess(changedRequestsCount, requestsCount);
            } else if (commandType === requestsDefinitions.REQUEST_COMMANDS.ProcessWaitlist) {
                var period = moment(waitlistPeriod.startDate).format("L") + "-" + moment(waitlistPeriod.endDate).format("L");
                requestsNotificationService.notifySubmitProcessWaitlistedRequestsSuccess(period);
            }
        }

        function monitorRunRequestWaitlist() {
            signalrSubscribe(
				{ DomainType: 'IRunRequestWaitlistEventMessage' }
				, RunRequestWaitlistEventHandler);
        }

        function signalrSubscribe(options, messsageHandler) {
            var $ = window.jQuery;
            var hub = $.connection.MessageBrokerHub;
            
            hub.client.onEventMessage = function (message) {
                messsageHandler(message);
            }
            
            $.connection.hub.url = "../signalr";
           
            $.connection.hub.start().done(function () {
                hub.server.addSubscription(options);
            }).fail(function(error) {
            });
                      
        };

        function formatDatePeriod(message) {
            vm.userTimeZone = CurrentUserInfo.CurrentUserInfo().DefaultTimeZone;
            var startDate = moment(message.StartDate.substring(1, message.StartDate.length)).tz(vm.userTimeZone).format("L");
            var endDate = moment(message.EndDate.substring(1, message.EndDate.length)).tz(vm.userTimeZone).format("L");
            return startDate + "-" + endDate;
        }

        function RunRequestWaitlistEventHandler(message) {
            if (vm.commandIdForMessage === message.TrackId) {
                var period = formatDatePeriod(message);
                requestsNotificationService.notifyProcessWaitlistedRequestsFinished(period);
            }
        }

        function onErrorMessages(errorMessages) {
            vm.disableInteraction = false;
            forceRequestsReloadWithoutSelection();

            errorMessages.forEach(function (errorMessage) {
                requestsNotificationService.notifyCommandError(errorMessage);
            });
        }

        //Todo: submit command failure doesn't give an error info, this parameter will be undefined.
        function onCommandError(error) {
            vm.disableInteraction = false;
            forceRequestsReloadWithoutSelection();
            requestsNotificationService.notifyCommandError(error);
        }
    }
})();