/* 
BuildTime is: 
2009-03-05 
10:35
*/ 
/* 
Trunk initiated: 
2009-02-25 
14:31
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: Devy Developer  
--Date: 2009-xx-xx  
--Desc: Because ...  
----------------  

----------------  
--Name: KJ
--Date: 2009-03-01 
--Desc: New language Russian  
----------------  
INSERT mart.language(language_id, language_name)
SELECT 1049,'Russian'
GO
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 1, N'All', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 2, N'Not Defined', N'?? ??????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 3, N'Hour', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 4, N'Half Hour', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 5, N'Interval', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 6, N'Day', N'????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 7, N'Week', N'??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 8, N'Month', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 9, N'Ready Time vs Scheduled Ready Time', N'????? ?????????? ? ????????? ?? ???????? ?????????? ? ??????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'????? ?????????? ? ????????? ?? ???????? ? ?????????? (??????? ????? ?? ? ????? ?????? ?????)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'????? ?????????? ? ????????? ? ??????????? ???????? ? ?????????? (?.?. ??? ????????? ?????)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'?????????? ?????? ? ?????? ??????? ?????? ???????/??????????? ??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'?????????? ? ?????????? ?????? ? ?????? ??????? ?????? ???????/??????????? ??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'?????????? ?????? ? ?????? ??????? ?????? ???????/?????????? ??????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 15, N'First Name', N'???')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 16, N'Last Name', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 17, N'Shift Start Time', N'????? ?????? ?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 18, N'Adherence', N'??????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 19, N'Monday', N'???????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 20, N'Tuesday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 21, N'Wednesday', N'?????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 22, N'Thursday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 23, N'Friday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 24, N'Saturday', N'???????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 25, N'Sunday', N'???????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 26, N'Activity', N'????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 27, N'Absence', N'??????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 28, N'Day Off', N'???????? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 29, N'Shift Category', N'????????? ????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 30, N'Extended Preference', N'??????????? ????????????')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 31, N'Weekday', N'???? ??????')
GO 
GO 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (75,'7.0.75') 
