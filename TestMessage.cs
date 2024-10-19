namespace TypedFaultDemo;

public class TestMessage
{
    public Guid TestId { get; set; }

    public bool ThrowInConsumer { get; set; }
}
