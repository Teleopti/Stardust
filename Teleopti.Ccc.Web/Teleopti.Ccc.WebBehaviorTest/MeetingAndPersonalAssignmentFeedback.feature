Feature: Preference Validation
	In order to clearly see preferences that collide with the pre scheduled personal shift or meeting.
	As an agent
	I want good feedback about personal shifts, meetings and the the preferences in collision   


Scenario: Can see a image about a meeting on the day
	Given There is a meeting on a day
	When I view the day
	Then I should see an icon about the meeting
	And I should see a tooltip with information about the meeting start and end time

Scenario: Can see a image about a personal assignment on the day
	Given I have a personal assingment
	When I view the day
	Then I should see an icon about the assignment
	And I should see a tooltip with information about the assignment start and end time

Scenario: Can see information about the limited range of possible external preferences because of meeting
	Given I have a shift bag with start times 8 to 9 and end times 16 to 17
	And I have a meeting between 15 and 17
	When I view the day
	Then I should see the end time boundry 17 to 17

Scenario: Can see information about the limited range of possible external preferences because of personal assingment
	Given I have a shift bag with start times 8 to 9 and end times 16 to 17
	And I have a personal assingment ending at 17
	When I view the day
	Then I should see the end time boundry 17 to 17

