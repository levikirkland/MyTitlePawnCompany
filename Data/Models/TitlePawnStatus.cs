namespace MyTitlePawnCompany.Data.Models
{
    public enum TitlePawnStatus
    {
        Pending = 0,      // Awaiting approval
        Approved = 1,     // Approved but not yet active
        Active = 2,       // Active loan
        Renewed = 3,      // Previous account renewed (historical record)
        PaidOff = 4,      // Fully paid
        Defaulted = 5,    // Past due and not caught up
        Closed = 6        // Account closed
    }
}
