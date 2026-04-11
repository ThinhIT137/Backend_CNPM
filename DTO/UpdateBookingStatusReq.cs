namespace backend.DTO
{
    public class UpdateBookingStatusReq
    {
        public string Status { get; set; } = null!; // "Confirmed", "Cancelled", "Completed"
    }
}
