Feature: Preferences Extended
	In order to view and submit when I prefer to work in more detail
	As an agent
	I want to view and submit extended preferences



	
Background:
    Given there is a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-06-24         |
	And there is a role with
	| Field                    | Value            |
	| Name                     | Access to mytime |
	| Access to mobile reports | false            |

Scenario: See extended preference Alternative
	Given I am a user
	And I have a role named 'Access to mytime'
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	And I have a workflow control set named 'Published schedule'
	And I have an extended preference on '2012-06-20'
	When I view preferences for date '2012-06-20'
	And I click the extended preference indication on '2012-06-20'
	Then I should see the extended preference on '2012-06-20'




Scenario: See indication of an extended preference
	Given I am an agent
	And I have an existing extended preference
	When I view preferences
	Then I should see that I have an existing extended preference

Scenario: See extended preference
	Given I am an agent
	And I have an existing extended preference
	When I view preferences
	And I click the extended preference indication
	Then I should see my existing extended preference

Scenario: See extended preference without permission
	Given I am an agent without access to extended preferences
	And I have an existing extended preference
	When I view preferences
	And I click the extended preference indication
	Then I should see my existing extended preference

