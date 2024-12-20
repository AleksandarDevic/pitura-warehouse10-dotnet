namespace Domain.Enums;

public enum JobStatus
{
    Unread = 0,
    ReadWithRequestedQuantity = 1,
    ReadWithDifferentQuantity = 2,
    NotReadNoArticle = 3,
    PhysicallyUnusableOrDamaged = 4
}