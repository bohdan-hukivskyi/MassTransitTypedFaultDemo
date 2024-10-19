using MassTransit;

namespace TypedFaultDemo;

public class TestFaultTypedMessageConsumer(ILogger<TestFaultTypedMessageConsumer> logger) : IConsumer<Fault<TestMessage>>
{
    public Task Consume(ConsumeContext<Fault<TestMessage>> context)
    {
        logger.LogInformation("Consumed typed FAULT for id {testId}", context.Message.Message.TestId);
        return Task.CompletedTask;
    }
}
