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
    PassMarkPercent INT NOT NULL DEFAULT 50
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
    (1, 'Welcome &amp; Setup', N'<h2>Welcome</h2><p>This module introduces the ASP.NET Web Forms platform, why the page lifecycle matters, and how server controls render.</p>', NULL, 1),
    (1, 'The Page Lifecycle', N'<h2>Page Lifecycle</h2><p>Init, Load, PostBack event handling, and PreRender determine every server-side control behaviour.</p>', 'https://www.youtube.com/embed/dQw4w9WgXcQ', 2);
GO

-- Module 2 of the ASP.NET course.
INSERT INTO Modules (CourseID, ModuleTitle, ModuleOrder) VALUES (1, 'Data Access', 2);
INSERT INTO Lessons (ModuleID, LessonTitle, ContentHTML, VideoUrl, LessonOrder) VALUES
    (2, 'ADO.NET &amp; Parameterised Queries', N'<h2>Parameterised Queries</h2><p>Every value is passed as a SqlParameter — the structural defence against SQL injection.</p>', NULL, 1);
GO

-- Quiz for Module 2 (the Data Access module), pass mark 70%.
INSERT INTO Quiz (ModuleID, QuizTitle, PassMarkPercent) VALUES (2, 'Data Access Check', 70);

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
    (3, 'Hello, Python', N'<h2>Hello, Python</h2><p>Python emphasises readable code. Start here.</p>', NULL, 1);
GO

-- Pretend the demo student has completed Lesson 1 and the module-2 quiz,
-- so dashboards/reports render with real data on first login.
INSERT INTO Progress (UserID, LessonID, IsCompleted, CompletionDate) VALUES
    (2, 1, 1, DATEADD(DAY, -2, GETDATE()));
INSERT INTO Results (UserID, QuizID, Score, IsPassed) VALUES
    (2, 1, 87.50, 1);
GO