using MassTransit;

namespace TypedFaultDemo;

internal class TestMessageConsumer(TestDbContext testDbContext) : IConsumer<TestMessage>
{
    public async Task Consume(ConsumeContext<TestMessage> context)
    {
        if (context.Message.ThrowInConsumer)
        {
            throw new InvalidOperationException("Throw requested by messaged inside consumer");
        }
        await testDbContext.AddAsync(new TestEntity()
        {
            Id = context.Message.TestId
        });
    }
}
