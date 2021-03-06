﻿USE [master]
GO
/****** Object:  Database [Event_Management]    Script Date: 3/5/2018 5:50:26 PM ******/
CREATE DATABASE [Event_Management]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Event_Management', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\Event_Management.mdf' , SIZE = 3264KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Event_Management_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL13.MSSQLSERVER\MSSQL\DATA\Event_Management_log.ldf' , SIZE = 1600KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Event_Management] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Event_Management].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Event_Management] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Event_Management] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Event_Management] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Event_Management] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Event_Management] SET ARITHABORT OFF 
GO
ALTER DATABASE [Event_Management] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [Event_Management] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Event_Management] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Event_Management] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Event_Management] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Event_Management] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Event_Management] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Event_Management] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Event_Management] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Event_Management] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Event_Management] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Event_Management] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Event_Management] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Event_Management] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Event_Management] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Event_Management] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Event_Management] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Event_Management] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Event_Management] SET  MULTI_USER 
GO
ALTER DATABASE [Event_Management] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Event_Management] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Event_Management] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Event_Management] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [Event_Management] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Event_Management', N'ON'
GO
ALTER DATABASE [Event_Management] SET QUERY_STORE = OFF
GO
USE [Event_Management]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [Event_Management]
GO
/****** Object:  Table [dbo].[Activites]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Activites](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NULL,
	[Description] [varchar](500) NOT NULL,
	[Thumbnail] [varchar](500) NOT NULL,
	[StartTime] [datetime] NOT NULL,
	[EndTime] [datetime] NOT NULL,
	[Address] [varchar](500) NOT NULL,
	[CreateON] [datetime] NULL,
	[Status] [bit] NULL,
	[ActivityTypeid] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ActivityTPYES]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ActivityTPYES](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppSetting]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppSetting](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Value] [varchar](max) NULL,
	[key] [varchar](max) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppUser]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUser](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [varchar](500) NOT NULL,
	[LastName] [varchar](500) NOT NULL,
	[Email] [varchar](500) NOT NULL,
	[Password] [varchar](1000) NOT NULL,
	[PhoneNumber] [varchar](500) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Attendes]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Attendes](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
	[Description] [varchar](500) NULL,
	[Thumbnail] [varchar](500) NULL,
	[FacebookURL] [varchar](1000) NULL,
	[TwitterURL] [varchar](1000) NULL,
	[InstagramURL] [varchar](1000) NULL,
	[AddedON] [datetime] NULL,
	[AddedBY] [int] NULL,
	[Status] [bit] NULL,
	[DeviceToken] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AttendesEvents]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AttendesEvents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AttendesID] [int] NOT NULL,
	[EventID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[BookMark]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[BookMark](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ActivityID] [int] NOT NULL,
	[AttendesID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Event]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Event](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ModeratePost] [bit] NULL,
	[GalacticEventId] [int] NULL,
	[EventName] [varchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FloorMapping]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FloorMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[PhotoURL] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FloorRegionMapping]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FloorRegionMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[FMid] [int] NOT NULL,
	[Description] [varchar](500) NULL,
	[X] [int] NULL,
	[Y] [int] NULL,
	[Width] [decimal](16, 2) NULL,
	[height] [decimal](16, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ForumSocialComments]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ForumSocialComments](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PostID] [int] NOT NULL,
	[AttendesID] [int] NOT NULL,
	[Comments] [varchar](500) NULL,
	[ApprovedON] [datetime] NULL,
	[ApprovedBY] [int] NULL,
	[CommentedON] [datetime] NULL,
	[ReplyerID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 3/5/2018 5:50:26 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[Text] [varchar](500) NULL,
	[AddedON] [datetime] NULL,
	[AddedBY] [int] NULL,
	[Status] [bit] NULL,
	[AttendesID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Permission]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Permission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[SectionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Photos]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Photos](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NULL,
	[URL] [varchar](1000) NULL,
	[UploadON] [datetime] NULL,
	[UploadBY] [int] NULL,
	[Description] [varchar](500) NULL,
	[Status] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PostType]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostType](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PostUserLike]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PostUserLike](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[PostID] [int] NOT NULL,
	[AttendesID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QRCode]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QRCode](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[Description] [varchar](500) NOT NULL,
	[AtivitityID] [int] NOT NULL,
	[WebURL] [varchar](1000) NOT NULL,
	[GeneratedON] [datetime] NULL,
	[GeneratedBY] [int] NULL,
	[Status] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[QRHistory]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[QRHistory](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[QRid] [int] NOT NULL,
	[AttendesID] [int] NOT NULL,
	[CommentON] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sections]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sections](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](500) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SocialForum]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SocialForum](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[URL] [varchar](1000) NULL,
	[UploadON] [datetime] NULL,
	[UploadBY] [int] NULL,
	[Description] [varchar](500) NULL,
	[PostTypeid] [int] NOT NULL,
	[ApprovedON] [datetime] NULL,
	[ApprovedBY] [int] NULL,
	[Status] [bit] NULL,
	[TotalLikes] [int] NULL,
	[AttendesID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Sponsors]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Sponsors](
	[ID] [int] NOT NULL,
	[Description] [varchar](500) NULL,
	[Thumbnail] [varchar](500) NULL,
	[DocURL] [varchar](1000) NULL,
	[Status] [bit] NULL,
	[Name] [varchar](500) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SponsorsEvents]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SponsorsEvents](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SponsorID] [int] NOT NULL,
	[EventID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Theme]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Theme](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[EventID] [int] NOT NULL,
	[SplashScreemURL] [varchar](1000) NULL,
	[AppBackgroundURL] [varchar](1000) NULL,
	[ButtonURL] [varchar](1000) NULL,
	[ButtonBackgroundColor] [varchar](10) NULL,
	[ButtonForegroundColor] [varchar](10) NULL,
	[LabelForegroundColor] [varchar](10) NULL,
	[HeadingForegroundColor] [varchar](10) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserEvent_Permission]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserEvent_Permission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[PermissionID] [int] NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserSession]    Script Date: 3/5/2018 5:50:27 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserSession](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AuthToken] [varchar](200) NULL,
	[Platform] [varchar](200) NULL,
	[UserID] [int] NULL,
	[AttendesID] [int] NULL,
	[CreateDate] [datetime] NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ForumSocialComments] ADD  DEFAULT ('0') FOR [ApprovedBY]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [AddedBY]
GO
ALTER TABLE [dbo].[QRCode] ADD  DEFAULT ('0') FOR [GeneratedBY]
GO
ALTER TABLE [dbo].[SocialForum] ADD  DEFAULT ('0') FOR [ApprovedBY]
GO
ALTER TABLE [dbo].[Activites]  WITH CHECK ADD FOREIGN KEY([ActivityTypeid])
REFERENCES [dbo].[ActivityTPYES] ([ID])
GO
ALTER TABLE [dbo].[Activites]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[AttendesEvents]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[AttendesEvents]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[BookMark]  WITH CHECK ADD FOREIGN KEY([ActivityID])
REFERENCES [dbo].[Activites] ([ID])
GO
ALTER TABLE [dbo].[BookMark]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[FloorMapping]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[FloorRegionMapping]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[FloorRegionMapping]  WITH CHECK ADD FOREIGN KEY([FMid])
REFERENCES [dbo].[FloorMapping] ([ID])
GO
ALTER TABLE [dbo].[ForumSocialComments]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[ForumSocialComments]  WITH CHECK ADD FOREIGN KEY([PostID])
REFERENCES [dbo].[SocialForum] ([ID])
GO
ALTER TABLE [dbo].[ForumSocialComments]  WITH CHECK ADD FOREIGN KEY([ReplyerID])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[Permission]  WITH CHECK ADD FOREIGN KEY([SectionID])
REFERENCES [dbo].[Sections] ([ID])
GO
ALTER TABLE [dbo].[Permission]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[Photos]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[PostUserLike]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[PostUserLike]  WITH CHECK ADD FOREIGN KEY([PostID])
REFERENCES [dbo].[SocialForum] ([ID])
GO
ALTER TABLE [dbo].[QRCode]  WITH CHECK ADD FOREIGN KEY([AtivitityID])
REFERENCES [dbo].[Activites] ([ID])
GO
ALTER TABLE [dbo].[QRCode]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[QRHistory]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[QRHistory]  WITH CHECK ADD FOREIGN KEY([QRid])
REFERENCES [dbo].[QRCode] ([ID])
GO
ALTER TABLE [dbo].[SocialForum]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[SocialForum]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[SocialForum]  WITH CHECK ADD FOREIGN KEY([PostTypeid])
REFERENCES [dbo].[PostType] ([ID])
GO
ALTER TABLE [dbo].[SponsorsEvents]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[SponsorsEvents]  WITH CHECK ADD FOREIGN KEY([SponsorID])
REFERENCES [dbo].[Sponsors] ([ID])
GO
ALTER TABLE [dbo].[Theme]  WITH CHECK ADD FOREIGN KEY([EventID])
REFERENCES [dbo].[Event] ([ID])
GO
ALTER TABLE [dbo].[UserEvent_Permission]  WITH CHECK ADD FOREIGN KEY([PermissionID])
REFERENCES [dbo].[Permission] ([ID])
GO
ALTER TABLE [dbo].[UserEvent_Permission]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[AppUser] ([Id])
GO
ALTER TABLE [dbo].[UserSession]  WITH CHECK ADD FOREIGN KEY([AttendesID])
REFERENCES [dbo].[Attendes] ([ID])
GO
ALTER TABLE [dbo].[UserSession]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[AppUser] ([Id])
GO
USE [master]
GO
ALTER DATABASE [Event_Management] SET  READ_WRITE 
GO
  ALTER TABLE [Event_Management2].[dbo].[SocialForum]
ADD DeleteRequest bit ;