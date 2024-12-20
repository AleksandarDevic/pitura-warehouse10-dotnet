namespace Domain.Enums;

public enum JobItemStatus
{
    Unread = 0,
    ReadWithRequestedQuantity = 1,
    ReadWithDifferentQuantity = 2,
    NotReadNoArticle = 3,
    PhysicallyUnusableOrDamaged = 4
}