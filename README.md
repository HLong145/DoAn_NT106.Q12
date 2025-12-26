# 🎮 FIGHTER X FIGHTER

**Môn học:** Lập trình mạng căn bản (NT106.Q12)  
**Đơn vị:** Trường Đại học Công nghệ Thông tin – ĐHQG TP.HCM  

---

## 🌟 Giới Thiệu Đồ Án

**Fighter x Fighter** là một trò chơi đối kháng **2 người chơi (Player vs Player – PvP)** theo phong cách **Pixel-Art cổ điển**, được phát triển như một **đồ án môn Lập trình mạng căn bản**.

Mục tiêu của dự án là xây dựng một **game hoàn chỉnh có khả năng chơi qua Internet**, áp dụng các kiến thức về:
- Lập trình Socket
- Giao thức mạng (TCP/UDP)
- Đồng bộ dữ liệu thời gian thực
- Mô hình Client–Server

---

## 💻 Công Nghệ & Kiến Trúc

### 🏗️ Kiến Trúc Tổng Thể
- **Mô hình:** Client – Server 3 tầng (Three-Tier Architecture)

### 🌐 Kiến Trúc Mạng (Network Stack)

| Tầng | Giao thức chính | Mục đích | Công nghệ sử dụng |
|----|---------------|---------|----------------|
| Giao tiếp chung | TCP/IP (JSON) | Đăng ký, Đăng nhập, Quản lý phòng, Chat Lobby/Room | C#, `TcpClient`, `TcpListener`, JSON (mã hóa AES) |
| Đồng bộ Real-time | UDP (Binary Protocol) | Đồng bộ vị trí, HP, hành động (attack, parry) với độ trễ thấp | C#, `UdpClient`, `UdpSocket` |

### ⚙️ Công Nghệ Chi Tiết
- **Ngôn ngữ / Nền tảng:** C# (.NET Framework)
- **Giao diện Client:** Windows Forms (WinForms)
- **Cơ sở dữ liệu:** SQL Server  
  (Lưu trữ thông tin người chơi, phòng chơi, trận đấu, Level/XP)
- **Bảo mật:**
  - Mã hóa mật khẩu bằng **SHA256**
  - Xác thực **Token**
  - **OTP qua Email** (sử dụng MailKit)

---

## 🔥 Tính Năng Nổi Bật (Key Features)

### 👤 Hệ Thống Tài Khoản
- Đăng ký, Đăng nhập, Đăng xuất
- Quên mật khẩu (xác thực OTP qua Email)

### 🏠 Hệ Thống Phòng Đấu
- Tạo phòng **Public** hoặc **Private** (có mật khẩu)
- Duyệt danh sách phòng
- Tham gia phòng bằng **Mã phòng**

### 💬 Giao Tiếp (Chat)
- **Global Chat:** nhắn tin với tất cả người chơi trong server
- **Room Chat:** nhắn tin với đối thủ trong phòng chờ

### 🎮 Gameplay Core
- Đồng bộ hóa **real-time** trạng thái nhân vật
- Xử lý logic va chạm, tính sát thương, combo

### ⚔️ Cơ Chế Chiến Đấu
- Di chuyển
- Tấn công thường
- **Đỡ (Parry):** miễn toàn bộ sát thương nhận vào
- **Lướt (Dash):** có *i-frame* (miễn sát thương tạm thời)
- Skill đặc trưng cho từng nhân vật

### 🧙 Hệ Thống Nhân Vật
- 4 nhân vật với chỉ số và kỹ năng riêng:
  - **Goatman Beserker**
  - **Bringer of Death**
  - **Elite Warrior**
  - **Scarlet Hunter**

### 📈 Level & XP
- Tính **XP** sau mỗi trận đấu dựa trên:
  - Kết quả trận
  - Hành động trong trận
- **Lên cấp (Level Up)** khi tích lũy đủ XP

---

## 🕹️ Luật Chơi Tóm Tắt

- **Hiệp đấu:** Tối đa 3 hiệp  
  → Hiệp kết thúc khi HP một người chơi về 0
- **Thắng trận:** Thắng 2/3 hiệp
- **Xử lý Disconnect:**  
  Nếu một người chơi bị mất kết nối, đối thủ sẽ **thắng trận (Forfeit)**

---

## 👨‍💻 Nhóm Thực Hiện

| Tên Sinh Viên | MSSV | Vai trò chính |
|--------------|------|--------------|
| **Lâm Tú Lan (Nhóm trưởng)** | 24520943 | Thiết kế UI/UX (Pixel 2D), Logic Gameplay, Kết nối UDP, Cơ chế Forfeit, Cơ chế đổi Avatar, Tích hợp âm thanh |
| **Phạm Quang Linh** | 24520968 | Xây dựng Hệ thống Level & XP, Logic tính điểm & kết thúc trận, Gửi mã OTP qua email |
| **Nguyễn Hoàng Long** | 24521005 | Logic & mã hóa mật khẩu, Tái cấu trúc hệ thống, Gameplay & Animation, Kết nối UDP |
| **Lục Vĩnh Kiệt** | 24520903 | Xây dựng & thiết lập Database (SQL Server), Quản lý truy vấn dữ liệu (CRUD) |
| **Huỳnh Thanh Duy** | 24520376 | Thiết kế giao diện & lập trình Server, Quản lý kết nối TCP, Xây dựng logic của hệ thống các phòng |

---

## 📌 Ghi Chú
Dự án được thực hiện với mục đích **học tập**, nghiên cứu và áp dụng các kiến thức về **lập trình mạng**, **đồng bộ thời gian thực** và **phát triển game PvP**.

---
## 🔽 Download

- 🌐 [Download Fighter X Fighter (Phiên bản Internet)](https://github.com/HLong145/DoAn_NT106.Q12/releases/tag/Internet_v1.0.0)
- 🏠 [Download Fighter X Fighter (Phiên bản LAN)](https://github.com/HLong145/DoAn_NT106.Q12/releases/tag/v1.0.0)



✨ *Cảm ơn bạn đã quan tâm đến dự án FIGHTER X FIGHTER!* ✨
