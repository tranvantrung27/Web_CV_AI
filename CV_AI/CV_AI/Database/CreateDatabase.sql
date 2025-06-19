-- Tạo cơ sở dữ liệu tuyển dụng
CREATE DATABASE JobRecruitmentDB;
GO

USE JobRecruitmentDB;
GO

-- Bảng Users (người dùng chung: ứng viên, nhà tuyển dụng, admin)
CREATE TABLE Users (
    ID_User INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL, -- "Candidate", "Employer", "Admin"
    CreatedAt DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

-- Bảng Categories (ngành nghề tuyển dụng)
CREATE TABLE Categories (
    ID_Category INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX)
);

-- Bảng Candidates (thông tin ứng viên)
CREATE TABLE Candidates (
    ID_Candidate INT PRIMARY KEY,
    Phone NVARCHAR(20),
    CV_Link NVARCHAR(255),
    FOREIGN KEY (ID_Candidate) REFERENCES Users(ID_User)
);

-- Bảng Employers (thông tin nhà tuyển dụng)
CREATE TABLE Employers (
    ID_Employer INT PRIMARY KEY,
    CompanyName NVARCHAR(100) NOT NULL,
    CompanyWebsite NVARCHAR(255),
    CompanyAddress NVARCHAR(255),
    Description NVARCHAR(MAX),
    FOREIGN KEY (ID_Employer) REFERENCES Users(ID_User)
);

-- Bảng JobPosts (tin tuyển dụng)
CREATE TABLE JobPosts (
    ID_JobPost INT PRIMARY KEY IDENTITY(1,1),
    ID_Employer INT NOT NULL,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    Requirements NVARCHAR(MAX),
    Location NVARCHAR(100),
    Salary NVARCHAR(50),
    PostedDate DATETIME DEFAULT GETDATE(),
    ExpirationDate DATETIME,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (ID_Employer) REFERENCES Employers(ID_Employer)
);

-- Bảng JobPostCategories (bảng trung gian giữa JobPosts và Categories – nhiều-nhiều)
CREATE TABLE JobPostCategories (
    ID_JobPost INT,
    ID_Category INT,
    PRIMARY KEY (ID_JobPost, ID_Category),
    FOREIGN KEY (ID_JobPost) REFERENCES JobPosts(ID_JobPost),
    FOREIGN KEY (ID_Category) REFERENCES Categories(ID_Category)
);

-- Bảng Applications (ứng tuyển công việc)
CREATE TABLE Applications (
    ID_Application INT PRIMARY KEY IDENTITY(1,1),
    ID_JobPost INT NOT NULL,
    ID_Candidate INT NOT NULL,
    AppliedDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending', -- Pending, Accepted, Rejected
    FOREIGN KEY (ID_JobPost) REFERENCES JobPosts(ID_JobPost),
    FOREIGN KEY (ID_Candidate) REFERENCES Candidates(ID_Candidate)
);

-- Bảng SavedJobs (ứng viên lưu tin tuyển dụng)
CREATE TABLE SavedJobs (
    ID_Candidate INT,
    ID_JobPost INT,
    SavedDate DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (ID_Candidate, ID_JobPost),
    FOREIGN KEY (ID_Candidate) REFERENCES Candidates(ID_Candidate),
    FOREIGN KEY (ID_JobPost) REFERENCES JobPosts(ID_JobPost)
);

-- Thêm dữ liệu mẫu cho Categories
INSERT INTO Categories (Name, Description) VALUES 
(N'Công nghệ thông tin', N'Các công việc liên quan đến lập trình, phát triển phần mềm'),
(N'Marketing', N'Các công việc liên quan đến tiếp thị, quảng cáo'),
(N'Tài chính', N'Các công việc liên quan đến tài chính, kế toán'),
(N'Giáo dục', N'Các công việc liên quan đến giảng dạy, đào tạo'),
(N'Y tế', N'Các công việc liên quan đến chăm sóc sức khỏe'),
(N'Du lịch', N'Các công việc liên quan đến du lịch, khách sạn');

-- Thêm tài khoản Admin mặc định
INSERT INTO Users (FullName, Email, PasswordHash, Role) VALUES 
(N'Admin', 'admin@jobrecruitment.com', 'AQAAAAEAACcQAAAAELbXpQGJgHsH8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q8Q==', 'Admin'); 