public class AddTimeEntryRequest
{
    public int TaskId { get; set; }
    public decimal HoursSpent { get; set; }
    public string? Note { get; set; }
}
