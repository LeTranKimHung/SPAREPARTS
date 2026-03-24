🏎️ Spare Parts Management System (SPMS)
A robust e-commerce and service management ecosystem built on ASP.NET Core 8.0. This platform provides a comprehensive solution for genuine automotive component retail, integrated VNPAY digital payments, and professional vehicle maintenance scheduling.

✨ Key Features
🛠️ For Customers (End-Users)
Premium Homepage: High-conversion UI featuring trending products and authentic customer testimonials.

Modern Shopping Experience: Intuitive cart system with automated shipping cost calculation based on geolocation.

VNPAY Integration: Secure online transactions using SHA-512 encryption (Sandbox environment supported).

Maintenance Booking: Smart scheduling form for repairs and servicing, including date-time selection and issue description.

Order Tracking: Real-time status monitoring (Pending, Shipping, Completed).

Personal Dashboard: Comprehensive history of past maintenance services and wishlisted items.

🛡️ For Administrators (Back-Office)
Inventory Management: Full CRUD operations for spare parts featuring an optimized Drag & Drop image upload system.

Service Appointment Queue: Streamlined workflow to review, accept, and update customer maintenance schedules.

Order & Cashflow Control: Centralized tracking for both COD and VNPAY digital transactions.

Marketing & Engagement: Manage discount coupons and moderate user reviews/feedback.

🚀 Tech Stack
Backend Framework: ASP.NET Core 8.0 MVC

Database: Entity Framework Core (Compatible with MySQL / SQL Server)

Frontend: Bootstrap 5, Font Awesome, Google Fonts, jQuery

Security: ASP.NET Identity (Authentication & Role-based Authorization)

Payment Gateway: VNPay .NET Integration Library

📦 Installation Guide
Clone the Repository:

Bash
git clone https://github.com/LeTranKimHung/SPAREPARTS.git
Database Configuration:
Navigate to appsettings.json and update the ConnectionStrings to match your local environment.

VNPAY Setup:
Ensure your TmnCode and HashSecret are correctly configured in appsettings.json using your VNPAY Sandbox credentials.

Apply Migrations:
Generate the database schema by running:

Bash
dotnet ef database update
Launch Application:

Bash
dotnet run
