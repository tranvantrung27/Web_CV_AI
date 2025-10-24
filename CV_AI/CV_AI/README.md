# 🚀 CV AI - Hệ thống Tuyển dụng & Tạo CV thông minh

## 📋 Giới thiệu
Ứng dụng web hỗ trợ tìm kiếm việc làm, tạo CV thông minh với AI và quản lý tuyển dụng.

## ✨ Tính năng chính

### 👤 Người dùng (Candidate)
- Tạo CV với hỗ trợ AI (Gemini)
- Tìm kiếm và lưu công việc yêu thích
- Ứng tuyển công việc trực tuyến
- Quản lý hồ sơ ứng tuyển
- Gửi góp ý/báo lỗi cho Admin

### 🏢 Nhà tuyển dụng (Employer)
- Đăng tin tuyển dụng
- Quản lý ứng viên
- Xem và tải CV ứng viên
- Cập nhật trạng thái đơn ứng tuyển

### 👨‍💼 Quản trị viên (Admin)
- Dashboard thống kê tổng quan
- Quản lý người dùng (kích hoạt/vô hiệu hóa/xóa)
- Quản lý tin tuyển dụng
- Quản lý đơn ứng tuyển
- Xem và phản hồi góp ý từ người dùng

## 🛠️ Công nghệ sử dụng
- **Backend**: ASP.NET Core 8.0 MVC
- **Database**: SQL Server (Entity Framework Core)
- **Authentication**: ASP.NET Identity + Google OAuth
- **AI Service**: Google Gemini API
- **Frontend**: Bootstrap 5, jQuery, SweetAlert2

## 📦 Cài đặt

### Yêu cầu hệ thống
- .NET 8.0 SDK
- SQL Server (LocalDB hoặc SQL Server Express)
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd Web_CV_AI/CV_AI/CV_AI
```

2. **Cấu hình database**
   - Mở file `appsettings.json`
   - Cập nhật connection string nếu cần:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=CV;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

3. **Tạo database và migration**
```bash
dotnet ef database update
```

4. **Chạy ứng dụng**

**Cách 1: Sử dụng script PowerShell (Khuyến nghị)**
```powershell
.\START_APP.ps1
```

**Cách 2: Chạy thủ công**
```bash
dotnet run
```

5. **Truy cập ứng dụng**
   - Mở trình duyệt: http://localhost:5068

## 🔐 Tài khoản mặc định

### Admin
- **Email**: `admin@gmail.com`
- **Password**: `Admin@123`

> ⚠️ **Lưu ý**: Hãy đổi mật khẩu admin ngay sau lần đăng nhập đầu tiên!

## 📁 Cấu trúc thư mục

```
CV_AI/
├── Controllers/          # API Controllers
│   ├── Admin/           # Admin controllers
│   ├── DichVu/          # Service controllers
│   └── HeThongQuanLy/   # Management controllers
├── Models/              # Data models
│   └── ViewModels/      # View models
├── Views/               # Razor views
│   ├── Admin/           # Admin views
│   ├── Feedback/        # Feedback views
│   └── Shared/          # Shared layouts
├── Data/                # Database context & seeder
├── Services/            # Business logic services
├── wwwroot/             # Static files
│   ├── css/
│   ├── js/
│   └── images/
└── Migrations/          # EF Core migrations
```

## 🎯 Hướng dẫn sử dụng

### Đăng nhập Admin
1. Truy cập: http://localhost:5068/Account/Login
2. Nhập email: `admin@gmail.com`
3. Nhập password: `Admin@123`
4. Sẽ tự động redirect đến Dashboard

### Quản lý Góp ý (Admin)
1. Từ Admin Dashboard, click menu "Feedback"
2. Hoặc truy cập: http://localhost:5068/AdminFeedback
3. Xem danh sách, lọc theo trạng thái, ưu tiên
4. Click "Chi tiết" để xem và phản hồi

### Gửi Góp ý (User)
1. Đăng nhập với tài khoản user
2. Truy cập: http://localhost:5068/Feedback
3. Click "Tạo góp ý mới"
4. Điền thông tin và gửi

## 🔧 Cấu hình

### Google OAuth
Cập nhật trong `Program.cs`:
```csharp
.AddGoogle(options =>
{
    options.ClientId = "YOUR_CLIENT_ID";
    options.ClientSecret = "YOUR_CLIENT_SECRET";
    options.CallbackPath = "/signin-google";
});
```

### Gemini AI
Cập nhật trong `appsettings.json`:
```json
{
  "Gemini": {
    "ApiKey": "YOUR_GEMINI_API_KEY"
  }
}
```

## 🐛 Xử lý lỗi thường gặp

### Lỗi: Port 5068 đã được sử dụng
```powershell
# Tìm process đang chiếm port
netstat -ano | findstr :5068

# Dừng process (thay <PID> bằng số PID tìm được)
taskkill /F /PID <PID>
```

### Lỗi: Database connection failed
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string trong `appsettings.json`
- Chạy lại migration: `dotnet ef database update`

### Lỗi: 401 Unauthorized
- Đảm bảo đã đăng nhập
- Xóa cookie và đăng nhập lại
- Kiểm tra session timeout (mặc định 30 phút)

## 📝 Changelog

### Version 1.0.0 (Latest)
- ✅ Thêm hệ thống Admin Dashboard
- ✅ Thêm chức năng Góp ý/Feedback
- ✅ Quản lý người dùng (Admin)
- ✅ Quản lý tin tuyển dụng (Admin)
- ✅ Tích hợp Google Gemini AI
- ✅ Authentication với Google OAuth
- ✅ Responsive design

## 👥 Đóng góp
Mọi đóng góp đều được chào đón! Vui lòng:
1. Fork repository
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License
[MIT License](LICENSE)

## 📧 Liên hệ
- Email: support@cvai.com
- Website: http://localhost:5068

---

**Chúc bạn sử dụng ứng dụng vui vẻ! 🎉**

