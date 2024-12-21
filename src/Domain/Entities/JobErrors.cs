using SharedKernel;

namespace Domain.Entities;

public static class JobErrors
{
    public static readonly Error NotFound = Error.NotFound(
        "Job.NotFound",
        "Job not found.");

    public static readonly Error JobInProgressNotFound = Error.NotFound(
        "JobInProgress.NotFound",
        "JobInProgress not found.");

    public static Error OperatorAlredyChoseJobInProgress(string jobDetails, int terminalId) => Error.Conflict(
        "JobInProgress.OperatorAlredyChoseJobInProgress",
        $"Operator has already chose job: {jobDetails} on terminal: {terminalId}.");

    public static readonly Error AlreadyAssigned = Error.Conflict(
        "Job.AlreadyAssigned",
        "Job has already assigned.");
}
