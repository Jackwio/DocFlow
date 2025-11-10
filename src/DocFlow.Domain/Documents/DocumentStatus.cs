namespace DocFlow.Documents;

public enum DocumentStatus
{
    Pending = 0,
    Classifying = 1,
    Classified = 2,
    Failed = 3,
    DeadLetter = 4,
    Expired = 5
}
