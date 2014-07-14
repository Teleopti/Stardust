Feature: Shift Trade Request Edit
	In order to handle existing shift trade requests
	As an agent
	I want to be able to take actions on my shift trade requests

Background:
	Given there is a role with
	| Field | Value                 |
	| Name  | Full access to mytime |
	And there is a workflow control set with
	| Field                            | Value                                     |
	| Name                             | Trade from tomorrow until 30 days forward |
	| Schedule published to date       | 2040-06-24                                |
	| Shift Trade sliding period start | 1                                         |
	| Shift Trade sliding period end   | 30                                        |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |	
	And OtherAgent has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And there are shift categories
	| Name  |
	| Day   |
	| Night |

Scenario: Close details when approving shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field    | Value         |
	| From       | Ashley Andeen	|
	| Pending  | True          |
	And I am viewing requests
	When I click on the existing request in the list
	And I click the Approve button on the shift request
	Then Details should be closed

Scenario: Can not approve or deny shift trade request created by me
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	| Field    | Value         |
	| To       | Ashley Andeen |
	| DateTo   | 2030-01-01    |
	| DateFrom | 2030-01-01    |
	| Pending  | True          |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should not see the approve button
	And I should not see the deny button

Scenario: Deny shift trade request
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field   | Value  |
	| From    | Ashley |
	| Pending | True   |
	And I am viewing requests
	When I click on the existing request in the list
	And I click the Deny button on the shift request
	Then Details should be closed

Scenario: Should not be able to delete received shift trade request
	Given I am an agent
	And I have received a shift trade request
	| Field		| Value		|
	| From		| Ashley	|
	When I view requests
	Then I should not see a delete button on the request

Scenario: Show name of sender of a received shifttrade
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have received a shift trade request
	| Field    | Value         |
	| From     | Ashley Andeen	|
	| DateTo   | 2030-01-01    |
	| DateFrom | 2030-01-01    |
	| Pending | True          |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see 'Ashley Andeen' as the sender of the request

Scenario: Show name of the person of a shifttrade that I have created
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category		| Day	           |
	And I have created a shift trade request
	| Field    | Value			|
	| To       | Ashley Andeen	|
	| DateTo   | 2030-01-01		|
	| DateFrom | 2030-01-01		|
	| Pending  | True			|
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see 'Ashley Andeen' as the receiver of the request

Scenario: Show schedules of the shift trade 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	| Lunch3HoursAfterStart | True             |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have created a shift trade request
	| Field    | Value         |
	| To       | Ashley Andeen	|
	| DateTo   | 2030-01-01    |
	| DateFrom | 2030-01-01    |
	| Pending  | True          |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see details with a schedule from
	| Field			| Value |
	| Start time	| 06:00 |
	| End time		| 16:00 |
	And I should see details with a schedule to
	| Field			| Value |
	| Start time	| 12:00 |
	| End time		| 22:00 |

Scenario: Show day off in a shifttrade
	Given I have the role 'Full access to mytime'
	And there is a dayoff with
	| Field | Value  |
	| Name  | DayOff |
	And there is a dayoff with
	| Field | Value		|
	| Name  | VacationButWithAReallyLongName |
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And 'I' have a day off with
	| Field | Value      |
	| Name  | DayOff     |
	| Date  | 2030-01-04 |
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And 'Ashley Andeen' have a day off with
	| Field | Value      |
	| Name  | VacationButWithAReallyLongName |
	| Date  | 2030-01-04 |
	And I have created a shift trade request
	| Field    | Value			|
	| To       | Ashley Andeen	|
	| DateTo   | 2030-01-04		|
	| DateFrom | 2030-01-04		|
	| Pending  | True			|
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see my details scheduled day off 'DayOff'
	And I should see other details scheduled day off 'VacationButWithAReallyLongName'

Scenario: Show subject of the shift trade in shifttrade details
Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have created a shift trade request
	| Field		| Value						|
	| To			| Ashley Andeen			|
	| DateTo		| 2030-01-01				|
	| DateFrom	| 2030-01-01				|
	| Pending	| True						|
	| Pending	| True						|
	| Subject	| Swap with me	|
	| Message	| CornercaseMessageWithAReallyReallyLongWordThatWillProbablyNeverHappenInTheRealWorldButItCausedATestIssueSoWePutItHereForTesting	|
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see details with subject 'Swap with me'
	And I should see details with message 'CornercaseMessageWithAReallyReallyLongWordThatWillProbablyNeverHappenInTheRealWorldButItCausedATestIssueSoWePutItHereForTesting'

Scenario: Show information that we dont show schedules in a shifttrade that isnt pending
	Given I have the role 'Full access to mytime'
	And I have created a shift trade request
	| Field			| Value		|
	| IsPending		| False		|
	And I am viewing requests
	When I click on the existing request in the list
	Then I should see details with message that tells the user that the status of the shifttrade is new
	And I should not see timelines

Scenario: Can not approve or deny shift trade request that is already approved
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field			| Value         |
	| From			| Ashley Andeen	|
	| Approved		| True          |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should not see the approve button
	And I should not see the deny button

Scenario: Resend referred shifttrade 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category		| Day	           |
	And I have created a shift trade request
	| Field				| Value				|
	| To				| Ashley Andeen		|
	| DateTo			| 2030-01-01		|
	| DateFrom			| 2030-01-01		|
	| IsPending			| True				|
	| HasBeenReferred	| True				|
	And I am viewing requests
	When I click on the existing request in the list
	And I click on shifttrade resend button
	Then I should see that the existing request is processing
	And I should not see resend shifttrade button for the request

Scenario: Cancel referred shifttrade 
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have created a shift trade request
	| Field				| Value         |
	| To				| Ashley Andeen	|
	| DateTo			| 2030-01-01    |
	| DateFrom			| 2030-01-01    |
	| Pending			| True          |
	| HasBeenReferred	| True          |
	And I am viewing requests
	When I click on the existing request in the list
	And I click on shifttrade cancel button
	Then I should not see any requests

Scenario: Do not show referred shifttrade to reciever
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have received a shift trade request
	| Field				| Value         |
	| From				| Ashley Andeen	|
	| DateTo			| 2030-01-01    |
	| DateFrom			| 2030-01-01    |
	| Pending			| True          |
	| HasBeenReferred	| True          |
	And I am viewing requests
	Then I should not see any requests

Scenario: Do not show resend and cancelbuttons to sender when shifttrade is not referred
	Given I have the role 'Full access to mytime'
	And I have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen have the workflow control set 'Trade from tomorrow until 30 days forward'
	And Ashley Andeen has a person period with
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 06:00 |
	| EndTime               | 2030-01-01 16:00 |
	| Shift category        | Day              |
	And 'Ashley Andeen' has a shift with
	| Field                 | Value            |
	| StartTime             | 2030-01-01 12:00 |
	| EndTime               | 2030-01-01 22:00 |
	| Shift category			| Day	           |
	And I have created a shift trade request
	| Field				| Value         |
	| To				| Ashley Andeen	|
	| DateTo			| 2030-01-01    |
	| DateFrom			| 2030-01-01    |
	| Pending			| True          |
	| HasBeenReferred	| False         |
	And I am viewing requests
	When I click on the existing request in the list
	Then I should not see resend shifttrade button for the request
	And I should not see cancel shifttrade button for the request

@OnlyRunIfEnabled('Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341')
Scenario:  Should input comment when seeing a shift trade request from other agent in pending status
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field   | Value  |
	| From    | Ashley |
	| Pending | True   |
	And I am viewing requests
	When I click on the existing request in the list
	Then I could edit message for the reason why I approve or deny this request

@OnlyRunIfEnabled('Request_GiveCommentWhenDenyOrApproveShiftTradeRequest_28341')
Scenario:  Should see the updated comment after approved
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field   | Value  |
	| From    | Ashley |
	| Pending | True   |
	And I am viewing requests
	When I click on the existing request in the list
	And I entered 'OK, you owe me a dinner' as comment
	And I click the Approve button on the shift request
	And I click on the existing request in the list
	Then I should see 'OK, you owe me a dinner' in message area
	
@ignore
Scenario:  Should see the updated comment after denied
	Given I have the role 'Full access to mytime'
	And I have received a shift trade request
	| Field   | Value  |
	| From    | Ashley |
	| Pending | True   |
	And I am viewing requests
	When I click on the existing request in the list
	And I entered 'Sorry, not this time' as comment
	And I click the Deny button on the shift request
	Then I should see 'Sorry, not this time' in message area