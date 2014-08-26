Feature: Diagnosis view
	In order to evaluate performance of broker
	As a developer
	I want to test, measure and view timings of messages
	
Background:
	Given I have a role with
	| Field | Value     |
	| Name  | Developer |
	And I have a person period with
    | Field      | Value      |	
	| Start date | 2013-06-01 |
	And there is an absence with
	| Field | Value    |
	| Name  | Vacation |

Scenario: Should send all messages
	When I am viewing the diagnosis view
	And I input a numberOfPings of 1000
	And I input a numberOfMessagesPerSecond of 100
	And I click 'send'
	Then I should see a count of 1000 messages sent
	And I should see a count of 0 messages left