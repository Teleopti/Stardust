-- Robin Karlsson, 2013-01-17
-- Drop everything related to the message broker

/****** Object:  StoredProcedure [msg].[generate_csharp]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_csharp]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_csharp]
GO

/****** Object:  StoredProcedure [msg].[generate_delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_delete]
GO

/****** Object:  StoredProcedure [msg].[generate_insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_insert]
GO

/****** Object:  StoredProcedure [msg].[generate_select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_select]
GO

/****** Object:  StoredProcedure [msg].[generate_update]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[generate_update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[generate_update]
GO

/****** Object:  StoredProcedure [msg].[sp_Address_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Address_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Address_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Address_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Address_Update]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Address_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Address_Update]
GO

/****** Object:  StoredProcedure [msg].[sp_ConcurrentUsers_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_ConcurrentUsers_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_ConcurrentUsers_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Configuration_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Configuration_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Configuration_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Configuration_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Configuration_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Configuration_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Configuration_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Configuration_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Configuration_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Configuration_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Configuration_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Configuration_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Configuration_Update]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Configuration_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Configuration_Update]
GO

/****** Object:  StoredProcedure [msg].[sp_current_users]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_current_users]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_current_users]
GO

/****** Object:  StoredProcedure [msg].[sp_Distinct_Heartbeats]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Distinct_Heartbeats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Distinct_Heartbeats]
GO

/****** Object:  StoredProcedure [msg].[sp_Event_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Event_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Event_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Event_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Event_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Event_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Event_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Event_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Event_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Event_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Event_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Event_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Events]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Events]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Events]
GO

/****** Object:  StoredProcedure [msg].[sp_Filter_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Filter_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Filter_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Filter_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Filter_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Filter_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Filter_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Filter_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Filter_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Filter_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Filter_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Filter_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Delete_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Delete_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Delete_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeat_Update]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeat_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeat_Update]
GO

/****** Object:  StoredProcedure [msg].[sp_Heartbeats]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Heartbeats]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Heartbeats]
GO

/****** Object:  StoredProcedure [msg].[sp_Log_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Log_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Log_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Log_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Log_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Log_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Log_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Log_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Log_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Log_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Log_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Log_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Logger]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Logger]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Logger]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Delete_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Delete_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Delete_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipt_Update]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipt_Update]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipt_Update]
GO

/****** Object:  StoredProcedure [msg].[sp_Receipts]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Receipts]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Receipts]
GO

/****** Object:  StoredProcedure [msg].[sp_Scavage_Events]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Scavage_Events]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Scavage_Events]
GO

/****** Object:  StoredProcedure [msg].[sp_Scavage_Subscribers]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Scavage_Subscribers]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Scavage_Subscribers]
GO

/****** Object:  StoredProcedure [msg].[sp_Subscriber_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Subscriber_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Subscriber_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Subscriber_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Subscriber_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Subscriber_Select_All]
GO

/****** Object:  StoredProcedure [msg].[sp_Users_Delete]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Users_Delete]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Users_Delete]
GO

/****** Object:  StoredProcedure [msg].[sp_Users_Insert]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Users_Insert]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Users_Insert]
GO

/****** Object:  StoredProcedure [msg].[sp_Users_Select]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Users_Select]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Users_Select]
GO

/****** Object:  StoredProcedure [msg].[sp_Users_Select_All]    Script Date: 01/17/2013 12:38:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[sp_Users_Select_All]') AND type in (N'P', N'PC'))
DROP PROCEDURE [msg].[sp_Users_Select_All]
GO

/****** Object:  View [msg].[vCurrentUsers]    Script Date: 01/17/2013 12:39:51 ******/
IF  EXISTS (SELECT * FROM sys.views WHERE object_id = OBJECT_ID(N'[msg].[vCurrentUsers]'))
DROP VIEW [msg].[vCurrentUsers]
GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Subscriber_IPAddress]') AND type = 'D')
BEGIN
ALTER TABLE [msg].[Subscriber] DROP CONSTRAINT [DF_Subscriber_IPAddress]
END

GO

IF  EXISTS (SELECT * FROM dbo.sysobjects WHERE id = OBJECT_ID(N'[DF_Subscriber_Port]') AND type = 'D')
BEGIN
ALTER TABLE [msg].[Subscriber] DROP CONSTRAINT [DF_Subscriber_Port]
END

GO

/****** Object:  Table [msg].[Address]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Address]') AND type in (N'U'))
DROP TABLE [msg].[Address]
GO

/****** Object:  Table [msg].[Configuration]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Configuration]') AND type in (N'U'))
DROP TABLE [msg].[Configuration]
GO

/****** Object:  Table [msg].[Event]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Event]') AND type in (N'U'))
DROP TABLE [msg].[Event]
GO

/****** Object:  Table [msg].[Filter]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Filter]') AND type in (N'U'))
DROP TABLE [msg].[Filter]
GO

/****** Object:  Table [msg].[Heartbeat]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Heartbeat]') AND type in (N'U'))
DROP TABLE [msg].[Heartbeat]
GO

/****** Object:  Table [msg].[Log]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Log]') AND type in (N'U'))
DROP TABLE [msg].[Log]
GO

/****** Object:  Table [msg].[Pending]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Pending]') AND type in (N'U'))
DROP TABLE [msg].[Pending]
GO

/****** Object:  Table [msg].[Receipt]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Receipt]') AND type in (N'U'))
DROP TABLE [msg].[Receipt]
GO

/****** Object:  Table [msg].[Subscriber]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Subscriber]') AND type in (N'U'))
DROP TABLE [msg].[Subscriber]
GO

/****** Object:  Table [msg].[UpdateType]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[UpdateType]') AND type in (N'U'))
DROP TABLE [msg].[UpdateType]
GO

/****** Object:  Table [msg].[Users]    Script Date: 01/17/2013 12:40:17 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[msg].[Users]') AND type in (N'U'))
DROP TABLE [msg].[Users]
GO

/****** Object:  Schema [msg]    Script Date: 01/17/2013 12:40:54 ******/
IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'msg')
DROP SCHEMA [msg]
GO

