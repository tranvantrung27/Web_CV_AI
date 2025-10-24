# ═══════════════════════════════════════════════════════════════
# SCRIPT TỰ ĐỘNG KHỞI ĐỘNG ỨNG DỤNG CV_AI
# ═══════════════════════════════════════════════════════════════

Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Cyan
Write-Host "║       🚀 ĐANG KHỞI ĐỘNG ỨNG DỤNG CV_AI...            ║" -ForegroundColor Cyan
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Cyan
Write-Host ""

# Bước 1: Kiểm tra port 5068 có bị chiếm không
Write-Host "🔍 Kiểm tra port 5068..." -ForegroundColor Yellow
$portCheck = netstat -ano | findstr :5068

if ($portCheck) {
    Write-Host "⚠️  Port 5068 đang bị chiếm!" -ForegroundColor Red
    
    # Lấy PID của process đang chiếm port
    $pid = ($portCheck | Select-String -Pattern '\s+(\d+)\s*$').Matches.Groups[1].Value
    
    if ($pid) {
        Write-Host "🔨 Đang dừng process (PID: $pid)..." -ForegroundColor Yellow
        taskkill /F /PID $pid 2>$null
        Start-Sleep -Seconds 2
        Write-Host "✅ Đã dừng process thành công!" -ForegroundColor Green
    }
}
else {
    Write-Host "✅ Port 5068 trống, sẵn sàng khởi động!" -ForegroundColor Green
}

Write-Host ""

# Bước 2: Dừng tất cả process dotnet cũ (nếu có)
Write-Host "🧹 Dọn dẹp các process dotnet cũ..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Where-Object {$_.MainWindowTitle -eq ""} | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process -Name "CV_AI" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 2
Write-Host "✅ Dọn dẹp hoàn tất!" -ForegroundColor Green
Write-Host ""

# Bước 3: Chuyển đến thư mục project
$projectPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $projectPath
Write-Host "📁 Đang ở thư mục: $projectPath" -ForegroundColor Cyan
Write-Host ""

# Bước 4: Build project (optional - uncomment nếu muốn build trước)
# Write-Host "🔨 Đang build project..." -ForegroundColor Yellow
# dotnet build --no-restore
# Write-Host ""

# Bước 5: Khởi động ứng dụng
Write-Host "╔════════════════════════════════════════════════════════╗" -ForegroundColor Green
Write-Host "║       🎉 BẮT ĐẦU KHỞI ĐỘNG ỨNG DỤNG...               ║" -ForegroundColor Green
Write-Host "╚════════════════════════════════════════════════════════╝" -ForegroundColor Green
Write-Host ""
Write-Host "⏳ Vui lòng đợi 15-20 giây để ứng dụng khởi động hoàn toàn..." -ForegroundColor Yellow
Write-Host ""
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "📌 SAU KHI KHỞI ĐỘNG XONG, TRUY CẬP:" -ForegroundColor White
Write-Host "   🌐 URL: http://localhost:5068" -ForegroundColor Green
Write-Host ""
Write-Host "🔐 ĐĂNG NHẬP ADMIN:" -ForegroundColor White
Write-Host "   📧 Email:    admin@gmail.com" -ForegroundColor Yellow
Write-Host "   🔑 Password: Admin@123" -ForegroundColor Yellow
Write-Host "═══════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️  Để DỪNG ứng dụng: Nhấn Ctrl+C" -ForegroundColor Red
Write-Host ""

# Chạy ứng dụng
dotnet run

