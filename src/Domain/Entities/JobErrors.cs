using SharedKernel;

namespace Domain.Entities;

public static class JobErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Job.NotFound",
        "Job not found.");

    public static Error NotCompleted(string jobDescription) => Error.NotFound(
        "Job.NotCompleted",
        $"Job {jobDescription} not completed.");

    public static readonly Error JobInProgressNotFound = Error.NotFound(
        "JobInProgress.NotFound",
        "JobInProgress not found.");

    public static Error OperatorAlredyChoseJobInProgress(string jobDetails, int terminalId) => Error.Conflict(
        "JobInProgress.OperatorAlredyChoseJobInProgress",
        $"Operator has already chose job: {jobDetails} on terminal: {terminalId}.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "Job.AlreadyAssigned",
        "Job has already assigned.");

    public static readonly Error AlreadyCompleted = Error.Conflict(
        "JobInProgress.AlreadyCompleted",
        $"JobInProgress has already completed.");

    public static readonly Error JobItemsNotReadedWithRequestedQuantity = Error.Conflict(
        "Job.JobItemsNotReadedWithRequestedQuantity",
        "Job Items haven't been read with requested quantity.");
}
