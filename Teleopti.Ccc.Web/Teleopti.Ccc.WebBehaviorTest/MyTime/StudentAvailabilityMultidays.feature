Feature: Student availability for multiple days
	In order to view and submit when I am available for work
	As a student agent
	I want to view and submit my availability

Background:
	Given there is a role with
	| Field                          | Value                          |
	| Name                           | Access to student availability |
	| Access to student availability | true                           |
	And I have a workflow control set with
	| Field                      | Value              |
	| Name                       | Published schedule |
	| Schedule published to date | 2012-06-24         |
	And I have a schedule period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |
	| Type       | Week       |
	| Length     | 1          |
	And I have a person period with 
	| Field      | Value      |
	| Start date | 2012-06-18 |

Scenario: Display student availabilities outside current period
	Given I have the role 'Access to student availability'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2012-06-27 |
	| Start time | 10:30      |
	| End time   | 11:00      |
	And I am viewing student availability for date '2012-06-20'
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-27 |
	| Start time | 10:30      |
	| End time   | 11:00      |

Scenario: Add student availability
	Given I have the role 'Access to student availability'
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 10:30 |
	| End time   | 11:00 |
	And I click the apply student availability button
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |

Scenario: Add student availability for multiple days
	Given I have the role 'Access to student availability'
	And I am viewing student availability for date '2012-06-20'
	When I select following days in student availability
	| Value      |
	| 2012-06-20 |
	| 2012-06-22 |
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 10:30 |
	| End time   | 11:00 |
	And I click the apply student availability button
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |
	And I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-22 |
	| Start time | 10:30      |
	| End time   | 11:00      |

Scenario: Add student availability with end time on next day
	Given I have the role 'Access to student availability'
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 16:30 |
	| End time   | 01:00 |
	| Next day   | true  |
	And I click the apply student availability button
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 16:30      |
	| End time   | 01:00 +1   |
	And I should not see the student availability on '2012-06-21'
	
Scenario: Cancel student availability editing
	Given I have the role 'Access to student availability'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 15:00 |
	And I click the close button
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |

Scenario: Replace student availability
	Given I have the role 'Access to student availability'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 15:00 |
	And I click the apply student availability button
	Then I should see the student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 13:30      |
	| End time   | 15:00      |

Scenario: Delete student availability
	Given I have the role 'Access to student availability'
	And I have a student availability with
	| Field      | Value      |
	| Date       | 2012-06-20 |
	| Start time | 10:30      |
	| End time   | 11:00      |
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the delete button in student availability
	Then I should not see the student availability on '2012-06-20'

Scenario: Add invalid student availability
	Given I have the role 'Access to student availability'
	And I am viewing student availability for date '2012-06-20'
	When I select day '2012-06-20' in student availability
	And I click the edit button in student availability
	And I input student availability with
	| Field      | Value |
	| Start time | 13:30 |
	| End time   | 11:00 |
	And I click the apply student availability button
	Then I should see a message 'InvalidTimeValue'
