namespace SmartGarage.DTOs
{
    public class PaymentRequestDTO
    {
        public string OrderCode { get; set; } = string.Empty;
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = "Tiền mặt"; // Tiền mặt, Chuyển khoản...
    }
}