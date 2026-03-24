using System;
using System.ComponentModel.DataAnnotations;

namespace SparePartsManagement.Models;

public class ServiceBooking
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Họ và tên")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [Phone]
    [Display(Name = "Số điện thoại")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Thương hiệu / Dòng xe")]
    public string CarModel { get; set; } = string.Empty;

    [Display(Name = "Biển số xe")]
    public string? LicensePlate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày hẹn")]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [Display(Name = "Khung giờ")]
    public string TimeSlot { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Dịch vụ yêu cầu")]
    public string ServiceDetails { get; set; } = string.Empty;

    public string? UserEmail { get; set; }

    [Display(Name = "Trạng thái")]
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public enum BookingStatus
{
    [Display(Name = "Chờ xác nhận")]
    Pending,
    [Display(Name = "Đã xác nhận")]
    Confirmed,
    [Display(Name = "Đã hoàn thành")]
    Completed,
    [Display(Name = "Đã hủy")]
    Cancelled
}
