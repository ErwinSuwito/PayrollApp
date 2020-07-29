USE [payroll]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
ALTER TABLE [dbo].[Users] DROP CONSTRAINT IF EXISTS [FK__Users__GroupID__2F10007B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[user_group]') AND type in (N'U'))
ALTER TABLE [dbo].[user_group] DROP CONSTRAINT IF EXISTS [FK__user_grou__RateI__2A4B4B5E]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shifts]') AND type in (N'U'))
ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT IF EXISTS [FK__Shifts__RateID__398D8EEE]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shifts]') AND type in (N'U'))
ALTER TABLE [dbo].[Shifts] DROP CONSTRAINT IF EXISTS [FK__Shifts__Location__3A81B327]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meeting_Group]') AND type in (N'U'))
ALTER TABLE [dbo].[Meeting_Group] DROP CONSTRAINT IF EXISTS [FK__Meeting_G__UserG__4316F928]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meeting_Group]') AND type in (N'U'))
ALTER TABLE [dbo].[Meeting_Group] DROP CONSTRAINT IF EXISTS [FK__Meeting_G__Meeti__4222D4EF]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meeting]') AND type in (N'U'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT IF EXISTS [FK__Meeting__RateID__3F466844]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Meeting]') AND type in (N'U'))
ALTER TABLE [dbo].[Meeting] DROP CONSTRAINT IF EXISTS [FK__Meeting__Locatio__3E52440B]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__UserID__46E78A0C]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__StartS__48CFD27E]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__Meetin__4AB81AF0]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__Locati__47DBAE45]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__EndShi__49C3F6B7]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Activity]') AND type in (N'U'))
ALTER TABLE [dbo].[Activity] DROP CONSTRAINT IF EXISTS [FK__Activity__Applic__4BAC3F29]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Users]
GO
/****** Object:  Table [dbo].[user_group]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[user_group]
GO
/****** Object:  Table [dbo].[Shifts]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Shifts]
GO
/****** Object:  Table [dbo].[Rate]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Rate]
GO
/****** Object:  Table [dbo].[Meeting_Group]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Meeting_Group]
GO
/****** Object:  Table [dbo].[Meeting]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Meeting]
GO
/****** Object:  Table [dbo].[Location]    Script Date: 7/29/2020 11:51:30 AM ******/
DROP TABLE IF EXISTS [dbo].[Location]
GO
/****** Object:  Table [dbo].[Global_Settings]    Script Date: 7/29/2020 11:51:31 AM ******/
DROP TABLE IF EXISTS [dbo].[Global_Settings]
GO
/****** Object:  Table [dbo].[Activity]    Script Date: 7/29/2020 11:51:31 AM ******/
DROP TABLE IF EXISTS [dbo].[Activity]
GO
