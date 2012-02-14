/* 
Trunk initiated: 
2009-04-29 
08:44
By: TOPTINET\johanr 
On TELEOPTI565 
*/ 
----------------  
--Name: KJ
--Date: 2009-03-01 
--Desc: New language Russian  
----------------  
DELETE FROM [mart].[language_translation] 
WHERE [language_id] = 1049
GO
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 1, N'All', N'Все')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 2, N'Not Defined', N'Не определено')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 3, N'Hour', N'Час')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 4, N'Half Hour', N'Полчаса')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 5, N'Interval', N'Интервал')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 6, N'Day', N'День')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 7, N'Week', N'Неделя')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 8, N'Month', N'Месяц')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 9, N'Ready Time vs Scheduled Ready Time', N'Время Готовности в сравнении со Временем Готовности в Расписании')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 10, N'Ready Time vs. Scheduled Time (incl time before and after shiftstart)', N'Время Готовности в сравнении со Временем в Расписании (включая время до и после начала смены)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 11, N'Ready Time  vs. Scheduled Contract Time (i.e excluding lunch)', N'Время Готовности в сравнении с Контрактным Временем в Расписании (т.е. без включения обеда)')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 12, N'Answered Calls Within Service Level Threshold / Offered Calls ', N'Отвеченные Вызовы в рамках Границы Уровня Сервиса/Поступившие вызовы')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 13, N'Answered and Abandoned Calls Within Service Level Threshold / Offered Calls', N'Отвеченные и Потерянные Вызовы в рамках Границы Уровня Сервиса/Поступившие вызовы')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 14, N'Answered Calls Within Service Level Threshold / Answered Calls', N'Отвеченные Вызовы в рамках Границы Уровня Сервиса/Отвеченные вызовы')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 15, N'First Name', N'Имя')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 16, N'Last Name', N'Фамилия')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 17, N'Shift Start Time', N'Время начала смены')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 18, N'Adherence', N'Соблюдение')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 19, N'Monday', N'Понедельник')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 20, N'Tuesday', N'Вторник')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 21, N'Wednesday', N'Среда')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 22, N'Thursday', N'Четверг')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 23, N'Friday', N'Пятница')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 24, N'Saturday', N'Суббота')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 25, N'Sunday', N'Воскресенье')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 26, N'Activity', N'Действие')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 27, N'Absence', N'Отсутствие')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 28, N'Day Off', N'Выходной день')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 29, N'Shift Category', N'Категория смен')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 30, N'Extended Preference', N'Расширенные Предпочтения')
INSERT [mart].[language_translation] ([language_id], [term_id], [term_english], [term_language]) VALUES (1049, 31, N'Weekday', N'День недели')
GO
GO 
 
 
PRINT 'Adding build number to database' 
INSERT INTO DatabaseVersion(BuildNumber, SystemVersion) VALUES (99,'7.0.99') 
