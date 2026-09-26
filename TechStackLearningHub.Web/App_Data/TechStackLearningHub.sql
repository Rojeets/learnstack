--
-- TechStack Learning Hub — schema + seed
--
-- PBKDF2 password-hash constants (FROZEN — must match AuthService.cs):
--   Algorithm : PBKDF2 (Rfc2898DeriveBytes)
--   PRF       : HMAC-SHA256 (HashAlgorithmName.SHA256)
--   Iterations: 100000
--   Salt size : 16 bytes
--   Hash size : 32 bytes
--   Storage   : PasswordSalt = base64(16-byte salt), PasswordHash = base64(32-byte derivation)
--
-- Re-runnable: drops and recreates the database for a clean seed.
--

IF DB_ID('TechStackLearningHub') IS NOT NULL
BEGIN
    ALTER DATABASE TechStackLearningHub SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE TechStackLearningHub;
END
GO

CREATE DATABASE TechStackLearningHub;
GO

USE TechStackLearningHub;
GO

-- Roles: separated into its own table (not an enum column) so RoleName is
-- data-driven and admins could add a role (e.g., "Instructor") later
-- without a schema change.
CREATE TABLE Roles (
    RoleID      INT IDENTITY(1,1) PRIMARY KEY,
    RoleName    NVARCHAR(20) NOT NULL UNIQUE   -- 'Student' or 'Admin'
);

CREATE TABLE Users (
    UserID        INT IDENTITY(1,1) PRIMARY KEY,
    Username      NVARCHAR(50)  NOT NULL UNIQUE,
    PasswordHash  NVARCHAR(256) NOT NULL,        -- base64 (32-byte PBKDF2-SHA256), never plaintext
    PasswordSalt  NVARCHAR(256) NOT NULL,         -- base64 (16-byte) unique salt per user
    Email         NVARCHAR(100) NOT NULL UNIQUE,
    RoleID        INT NOT NULL FOREIGN KEY REFERENCES Roles(RoleID),
    IsActive      BIT NOT NULL DEFAULT 1,
    CreatedDate   DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Courses (
    CourseID     INT IDENTITY(1,1) PRIMARY KEY,
    CourseName   NVARCHAR(150) NOT NULL,
    TechStack    NVARCHAR(50)  NOT NULL,          -- e.g. 'ASP.NET', 'React', 'Python'
    Description  NVARCHAR(1000) NULL,
    IsPublished  BIT NOT NULL DEFAULT 0,           -- lets admins draft before releasing
    CreatedDate  DATETIME NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Modules (
    ModuleID     INT IDENTITY(1,1) PRIMARY KEY,
    CourseID     INT NOT NULL FOREIGN KEY REFERENCES Courses(CourseID) ON DELETE CASCADE,
    ModuleTitle  NVARCHAR(150) NOT NULL,
    ModuleOrder  INT NOT NULL
);

CREATE TABLE Lessons (
    LessonID     INT IDENTITY(1,1) PRIMARY KEY,
    ModuleID     INT NOT NULL FOREIGN KEY REFERENCES Modules(ModuleID) ON DELETE CASCADE,
    LessonTitle  NVARCHAR(150) NOT NULL,
    ContentHTML  NVARCHAR(MAX) NULL,
    VideoUrl     NVARCHAR(300) NULL,
    NotesFilePath NVARCHAR(300) NULL,
    LessonOrder  INT NOT NULL
);

CREATE TABLE Quiz (
    QuizID          INT IDENTITY(1,1) PRIMARY KEY,
    ModuleID        INT NOT NULL FOREIGN KEY REFERENCES Modules(ModuleID) ON DELETE CASCADE,
    QuizTitle       NVARCHAR(150) NOT NULL,
    PassMarkPercent INT NOT NULL DEFAULT 50,
    -- Server-authoritative attempt limit in minutes. NULL means "no limit
    -- authored", which readers resolve to QuizBLL.DefaultDurationMinutes, so a
    -- NULL can never be mistaken for a zero-length attempt.
    DurationMinutes INT NULL CHECK (DurationMinutes IS NULL OR (DurationMinutes BETWEEN 1 AND 240))
);

CREATE TABLE Questions (
    QuestionID    INT IDENTITY(1,1) PRIMARY KEY,
    QuizID        INT NOT NULL FOREIGN KEY REFERENCES Quiz(QuizID) ON DELETE CASCADE,
    QuestionText  NVARCHAR(500) NOT NULL,
    Marks         INT NOT NULL DEFAULT 1
);

CREATE TABLE Answers (
    AnswerID    INT IDENTITY(1,1) PRIMARY KEY,
    QuestionID  INT NOT NULL FOREIGN KEY REFERENCES Questions(QuestionID) ON DELETE CASCADE,
    AnswerText  NVARCHAR(300) NOT NULL,
    IsCorrect   BIT NOT NULL DEFAULT 0
);

CREATE TABLE Results (
    ResultID     INT IDENTITY(1,1) PRIMARY KEY,
    UserID       INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    QuizID       INT NOT NULL FOREIGN KEY REFERENCES Quiz(QuizID),
    Score        DECIMAL(5,2) NOT NULL,            -- percentage score, e.g. 87.50
    AttemptDate  DATETIME NOT NULL DEFAULT GETDATE(),
    IsPassed     BIT NOT NULL
);

CREATE TABLE Progress (
    ProgressID      INT IDENTITY(1,1) PRIMARY KEY,
    UserID          INT NOT NULL FOREIGN KEY REFERENCES Users(UserID),
    LessonID        INT NOT NULL FOREIGN KEY REFERENCES Lessons(LessonID),
    IsCompleted     BIT NOT NULL DEFAULT 0,
    CompletionDate  DATETIME NULL,
    CONSTRAINT UQ_User_Lesson UNIQUE (UserID, LessonID)
);

-- Password-reset requests. Lives with the other Users child tables (Results,
-- Progress) because a token is meaningless without its account: CASCADE means
-- deleting a user takes their outstanding reset links with them instead of
-- leaving live credentials pointing at a UserID that no longer exists.
CREATE TABLE PasswordResetTokens (
    TokenID      INT IDENTITY(1,1) PRIMARY KEY,
    UserID       INT NOT NULL FOREIGN KEY REFERENCES Users(UserID) ON DELETE CASCADE,
    -- SHA-256 of the emailed token, never the token itself. The 32-byte token
    -- is CSPRNG output, so it cannot be brute-forced, but a table read would
    -- otherwise hand out every live reset link in the system; storing only the
    -- hash means a database leak yields nothing replayable. Same reasoning as
    -- the per-user salt on Users.PasswordHash.
    TokenHash    NVARCHAR(256) NOT NULL,
    ExpiresAt    DATETIME NOT NULL,
    -- NULL while the token is still redeemable. Rows are never deleted on use:
    -- stamping the column is what makes redemption single-use (the
    -- "AND UsedAt IS NULL" guard on the UPDATE), and keeping the row means a
    -- replay is refused for a reason that can be audited.
    UsedAt       DATETIME NULL,
    CreatedDate  DATETIME NOT NULL DEFAULT GETDATE()
);
-- UNIQUE because the lookup is by hash and a duplicate would make
-- "which row did this token match?" ambiguous; lookups are by hash only, so
-- this also serves as the covering index for them.
CREATE UNIQUE INDEX UX_PasswordResetTokens_TokenHash ON PasswordResetTokens(TokenHash);
-- Supports "invalidate every outstanding token for this user", which runs on
-- each new request and at the end of each successful reset.
CREATE INDEX IX_PasswordResetTokens_UserID ON PasswordResetTokens(UserID);

-- Seed the two roles the application logic depends on by name.
INSERT INTO Roles (RoleName) VALUES ('Student'), ('Admin');
GO

--
-- SEED DATA
--

-- Users. Hashes produced by the exact PBKDF2-SHA256 constants above.
-- admin / Admin@123   (role: Admin)
-- student / Student@123 (role: Student)
INSERT INTO Users (Username, PasswordHash, PasswordSalt, Email, RoleID)
VALUES
    ('admin',   'KzWYtBZbap3or+gKnpQREMRbWPGaNBSkEmjHONJbpPE=', 'tv4adq9xfUo4nPCsrPIRmg==', 'admin@techstack.local', 2),
    ('student', 'SOMjaElPxYQ2FCOEXK5cvKQfZBcQYTl5EdScgc+tm38=', 'WG6uKcUUP5TGAoQ2ByHhZw==', 'student@techstack.local', 1);
GO

-- Courses.
-- 1: ASP.NET (published, full demo content: 2 modules / 3 lessons / 1 quiz)
-- 2: Python  (published, light content for the catalogue filter)
-- 3: Flutter (DRAFT / unpublished — must never leak to the student catalogue)
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES
    ('Building Web Apps with ASP.NET Web Forms', 'ASP.NET', 'A hands-on path from page lifecycle to parameterised data access using the classic three-tier architecture.', 1),
    ('Python Programming Fundamentals',         'Python',  'General-purpose programming with a data and automation angle.', 1),
    ('Flutter: Cross-Platform UI',              'Flutter', 'Future-facing mobile UI with a single Dart codebase.', 0);
GO

-- Module 1 of the ASP.NET course.
INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (1, 'The Web Forms Platform', 1);
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (1, 'Welcome &amp; Setup', N'<h2>Welcome</h2><p>This module introduces the ASP.NET Web Forms platform, why the page lifecycle matters, and how server controls render.</p>', 'https://www.youtube.com/embed/64_Y_S0ihlw', 1),
    (1, 'The Page Lifecycle', N'<h2>Page Lifecycle</h2><p>Init, Load, PostBack event handling, and PreRender determine every server-side control behaviour.</p>', 'https://www.youtube.com/embed/jGwexlejAcM', 2);
GO

-- Module 2 of the ASP.NET course.
INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (1, 'Data Access', 2);
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (2, 'ADO.NET &amp; Parameterised Queries', N'<h2>Parameterised Queries</h2><p>Every value is passed as a SqlParameter — the structural defence against SQL injection.</p>', NULL, 1);
GO

-- Quiz for Module 2 (the Data Access module), pass mark 70%.
-- NULL duration on purpose: this row exercises the default-limit fallback.
INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (2, 'Data Access Check', 70, NULL);

-- Q1: single-answer, 2 marks.
INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (1, 'Which technique structurally prevents SQL injection in ADO.NET?', 2);
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (1, 'Parameterised SqlCommand objects', 1),
    (1, 'Escaping apostrophes in user input', 0),
    (1, 'String concatenation into the query', 0),
    (1, 'Stored procedures only', 0);

-- Q2: multi-answer, 3 marks.
INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (1, 'Which of the following are ASP.NET Web Forms server controls?', 3);
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (2, 'GridView', 1),
    (2, 'Repeater', 1),
    (2, 'FormView', 1),
    (2, 'React Component', 0);
GO

-- Python course: one intro module/lesson so the catalogue shows a real course.
INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (2, 'Getting Started', 1);
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (3, 'Hello, Python', N'<h2>Hello, Python</h2><p>Python emphasises readable code. Start here.</p>', 'https://www.youtube.com/embed/rfscVS0vtbw', 1);
GO

-- Flutter course: drafted and unpublished, so the admin publish/unpublish
-- toggle has something to act on. One real module and lesson behind the
-- draft state.
INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (3, 'Dart and Widgets', 1);
DECLARE @ModFlutter INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModFlutter, 'Your First Flutter App', N'<h2>Your First Flutter App</h2><p>Flutter renders one Dart codebase to both mobile platforms. This module walks a first app from project creation to a running widget tree.</p>', 'https://www.youtube.com/embed/VPvVD8t02U8', 1);
GO

-- ================= REACT =================
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES ('React Fundamentals', 'React', 'Learn JSX, components, props, state, and hooks by building real projects.', 1);
DECLARE @CourseReact INT = SCOPE_IDENTITY();

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseReact, 'React Basics', 1);
DECLARE @ModReact1 INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModReact1, 'JSX, Components, and Props', N'<p>JSX lets you write HTML-like markup inside JavaScript. This lesson builds reusable components and passes data down with props.</p>', 'https://www.youtube.com/embed/x4rFhThSX04', 1);

INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (@ModReact1, 'React Basics Quiz', 60, 15);
DECLARE @QuizReact1 INT = SCOPE_IDENTITY();

INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (@QuizReact1, 'What does JSX allow you to write?', 1);
DECLARE @QReact1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (@QReact1, 'HTML-like syntax inside JavaScript', 1),
    (@QReact1, 'CSS inside HTML', 0),
    (@QReact1, 'SQL inside JavaScript', 0),
    (@QReact1, 'XML schemas', 0);
GO

-- ================= JAVA =================
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES ('Java Programming Essentials', 'Java', 'Variables, control flow, arrays, collections, and object-oriented programming in Java.', 1);
DECLARE @CourseJava INT = SCOPE_IDENTITY();

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseJava, 'Java Fundamentals', 1);
DECLARE @ModJava1 INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModJava1, 'Variables, Data Types, and Operators', N'<p>Java''s primitive types, variable declarations, and the operator set.</p>', 'https://www.youtube.com/embed/A74TOX803D0', 1);

INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (@ModJava1, 'Java Fundamentals Quiz', 60, 5);
DECLARE @QuizJava1 INT = SCOPE_IDENTITY();

INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (@QuizJava1, 'Which keyword is used to inherit a class in Java?', 1);
DECLARE @QJava1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (@QJava1, 'extends', 1),
    (@QJava1, 'implements', 0),
    (@QJava1, 'inherits', 0),
    (@QJava1, 'super', 0);
GO

-- ================= NODE.JS =================
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES ('Node.js Fundamentals', 'Node.js', 'The Node.js runtime, modules, npm, and building a basic web server.', 1);
DECLARE @CourseNode INT = SCOPE_IDENTITY();

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseNode, 'Node.js Core Concepts', 1);
DECLARE @ModNode1 INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModNode1, 'What is Node? Modules and npm', N'<p>The Node runtime executes JavaScript outside the browser. This lesson covers the runtime, the module system, and installing packages with npm.</p>', 'https://www.youtube.com/embed/gG3pytAY2MY', 1);

INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (@ModNode1, 'Node.js Basics Quiz', 60, 20);
DECLARE @QuizNode1 INT = SCOPE_IDENTITY();

INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (@QuizNode1, 'What does npm stand for?', 1);
DECLARE @QNode1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (@QNode1, 'Node Package Manager', 1),
    (@QNode1, 'New Programming Module', 0),
    (@QNode1, 'Node Process Manager', 0),
    (@QNode1, 'Network Package Manager', 0);
GO

-- ================= PHP/LARAVEL =================
-- TechStack must match CourseBLL.KnownTechStacks exactly or the admin
-- validator rejects the course on any later edit.
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES ('PHP and Laravel Fundamentals', 'PHP/Laravel', 'PHP syntax, arrays, classes, and an introduction to the Laravel framework.', 1);
DECLARE @CoursePhp INT = SCOPE_IDENTITY();

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CoursePhp, 'PHP Language Basics', 1);
DECLARE @ModPhp1 INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModPhp1, 'Syntax, Arrays, and Functions', N'<p>PHP variables, arrays, and functions, and how the language fits into a request/response cycle.</p>', 'https://www.youtube.com/embed/OK_JCtrrv-c', 1);

INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (@ModPhp1, 'PHP Basics Quiz', 60, 12);
DECLARE @QuizPhp1 INT = SCOPE_IDENTITY();

INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (@QuizPhp1, 'In PHP, which construct declares a function?', 1);
DECLARE @QPhp1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (@QPhp1, 'function', 1),
    (@QPhp1, 'fun', 0),
    (@QPhp1, 'sub', 0),
    (@QPhp1, 'method', 0);
GO

-- ================= ANDROID (KOTLIN) =================
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES ('Android Development with Kotlin', 'Android (Kotlin)', 'Kotlin syntax, activities, layouts, and building a first Android app.', 1);
DECLARE @CourseKotlin INT = SCOPE_IDENTITY();

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseKotlin, 'Kotlin and Android Basics', 1);
DECLARE @ModKotlin1 INT = SCOPE_IDENTITY();

INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModKotlin1, 'Your First Android App', N'<p>Kotlin basics, the activity lifecycle, and building and running a first Android project.</p>', 'https://www.youtube.com/embed/F9UC9DY-vIU', 1);

INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent, DurationMinutes) VALUES (@ModKotlin1, 'Kotlin Basics Quiz', 60, 8);
DECLARE @QuizKotlin1 INT = SCOPE_IDENTITY();

INSERT INTO Questions (QuizID, QuestionText, Marks) VALUES (@QuizKotlin1, 'Which file lists an Android app''s permissions in a Kotlin project?', 1);
DECLARE @QKotlin1 INT = SCOPE_IDENTITY();
INSERT INTO Answers (QuestionID, AnswerText, IsCorrect) VALUES
    (@QKotlin1, 'AndroidManifest.xml', 1),
    (@QKotlin1, 'build.gradle', 0),
    (@QKotlin1, 'MainActivity.kt', 0),
    (@QKotlin1, 'settings.gradle', 0);
GO

-- A second course in two of the stacks. Not filler: the admin dashboard's
-- "courses by tech stack" bars normalise to the largest stack, so a seed with
-- exactly one course per stack renders eight identical full-width bars and
-- shows nothing.
INSERT INTO Courses (CourseName, TechStack, Description, IsPublished)
VALUES
    ('Master Pages, User Controls, and Validation', 'ASP.NET', 'Reusable page templates, user controls, and server-side validation in Web Forms.', 1),
    ('React with Hooks and State', 'React', 'useState, useEffect, and lifting state up, with refactoring exercises.', 1);

DECLARE @CourseAspNet2 INT = (SELECT CourseID FROM Courses WHERE CourseName = 'Master Pages, User Controls, and Validation');
DECLARE @CourseReact2 INT = (SELECT CourseID FROM Courses WHERE CourseName = 'React with Hooks and State');

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseAspNet2, 'Reusable UI', 1);
DECLARE @ModAspNet2 INT = SCOPE_IDENTITY();
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModAspNet2, 'Reusable Page Templates', N'<p>Master pages define one layout for a whole section; user controls encapsulate a reusable fragment with its own events.</p>', 'https://www.youtube.com/embed/64_Y_S0ihlw', 1);

INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (@CourseReact2, 'Hooks and State', 1);
DECLARE @ModReact2 INT = SCOPE_IDENTITY();
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (@ModReact2, 'useState and useEffect', N'<p>Function components hold state with useState and synchronise with external systems through useEffect.</p>', 'https://www.youtube.com/embed/x4rFhThSX04', 1);
GO

-- Pretend the demo student has completed Lesson 1 and the module-2 quiz,
-- so dashboards/reports render with real data on first login. LessonID 1 is
-- resolved by name because the seed above inserts by SCOPE_IDENTITY, and
-- QuestionID 1 is fixed in the hand-written section.
DECLARE @LessonOne INT = (SELECT LessonID FROM Lessons WHERE LessonTitle = 'Welcome &amp; Setup');
INSERT INTO Progress (UserID, LessonID, IsCompleted, CompletionDate) VALUES
    (2, @LessonOne, 1, DATEADD(DAY, -2, GETDATE()));
INSERT INTO Results (UserID, QuizID, Score, IsPassed) VALUES
    (2, 1, 87.50, 1);
GO