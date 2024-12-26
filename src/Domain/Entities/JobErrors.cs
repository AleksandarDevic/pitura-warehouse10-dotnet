using SharedKernel;

namespace Domain.Entities;

public static class JobErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Job.NotFound",
        "Posao nije pronađen.");

    public static Error NotCompleted(string jobDescription) => Error.Conflict(
        "Job.NotCompleted",
        $"Posao {jobDescription} nije završen.");

    public static readonly Error JobInProgressNotFound = Error.NotFound(
        "JobInProgress.NotFound",
        "Posao u toku nije pronađen.");

    public static Error OperatorAlredyChoseJobInProgress(string jobDetails, int terminalId) => Error.Conflict(
        "JobInProgress.OperatorAlredyChoseJobInProgress",
        $"Operator je već izabrao posao: {jobDetails} na terminalu: {terminalId}.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "Job.AlreadyAssigned",
        "Posao je već dodeljen.");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        "Job.AlreadyCompleted",
        $"Posao je već završen.");

    public static readonly Error JobInProgressAlreadyCompleted = Error.Conflict(
        "JobInProgress.AlreadyCompleted",
        $"Posao u toku je već završen.");

    public static readonly Error JobItemsNotReadedWithRequestedQuantity = Error.Conflict(
        "Job.JobItemsNotReadedWithRequestedQuantity",
        "Stavke posla nisu očitane sa traženom količinom.");

    public static readonly Error JobItemAlreadyReaded = Error.Conflict(
        "JobItem.AlreadyReaded",
        "Stavka posla je već očitana.");

    public static readonly Error JobItemBadQuanity = Error.Conflict(
        "JobItem.BadQuanity",
        "Stavka posla ima loše unetu količinu.");

    public static readonly Error JobItemRequestedAndReadedValueNotMatch = Error.Conflict(
        "JobItem.JobItemRequestedAndReadedValueNotMatch",
        "Stavka posla ima različite očitane vrednosti u odnosu na tražene.");

    public static readonly Error JobItemNotFound = Error.NotFound(
        "JobItem.NotFound",
        "Stavka posla nije pronađena.");
}
