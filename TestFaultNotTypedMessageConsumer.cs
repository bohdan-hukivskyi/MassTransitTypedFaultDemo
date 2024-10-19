using MassTransit;

namespace TypedFaultDemo;

public class TestFaultNotTypedMessageConsumer(ILogger<TestFaultNotTypedMessageConsumer> logger) : IConsumer<Fault>
{
    public Task Consume(ConsumeContext<Fault> context)
    {
        logger.LogInformation("Consumed NOTTYPED FAULT");
        return Task.CompletedTask;
    }
}
