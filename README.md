# Demonstration of MassTransit FAULT event publication behavior

## Bug description

### Version

8.x

### On which operating system(s) are you experiencing the issue?

Linux, Windows

### Using which broker(s) did you encounter the issue?

RabbitMQ

### What are the steps required to reproduce the issue?

1. Configure MassTransit using EntityFramework outbox and RabbitMq.
2. Create TestSaveCommand
3. Create TestSaveCommandConsumer that adds wrong data to DbContext, for example with an index uniqueness violation.
4. Create Fault\<TestSaveCommand\> consumer.
...


### What is the expected behavior?

IConsumer<Fault<TestSaveCommand>> receive fault event notification, when TestSaveCommandConsumer failed.

Conceptually, it is very nice and convenient, especially when creating sagas, to be able to subscribe (in standard typed way) to the fault notification of processing a certain message, with access to the body of this message.

### What actually happened?

IConsumer<Fault<TestSaveCommand>> does not receive fault event notification, when TestSaveCommandConsumer failed.

If an exception occurs within a TestSaveCommandConsumer (e.g. throw new IvalidOperationException() in consumer body), the IConsumer\<Fault\<TestSaveCommand\>\> will receive a notification.

But if an exception occurs outside the consumer, for example when saving the context or committing a transaction, then IConsumer\<Fault\<TestSaveCommand\>\> will not receive it. (In such cases, only untyped events are published: ReceiveFault and Fault)

Untyped Fault not very suitable for many scenarios and requires much more effort to implement the same logic.

In addition, the lack of guarantees that a typed Fault\<T\> will be published for all types business errors makes this notification quite dangerous to use.

Even if it is impossible to deserialize the message, this event should be published, but without a body with the corresponding flag. For other types of problems, this event can always be published in full.
Then it will be really convenient, reliable and predictable.


## Demo detail

### How to run

1. Install and run Docker Desktop.
2. Download and run sample project `dotnet run` from solution root.

### Demo env

MassTransit v8.3
MassTransit EntityFramework outbox
TestContainers for MsSqlServer
TestContainers for RabbitMq

### Demo scenario

1. I publish the correct message, which leads to the saving of a simple entity in the database
2. I publish the message with error flag, which causes the consumer to crash and the typed and non-typed consumers to receive appropriate notifications (Correct fault behavior)
3. I publish the message with duplicated entity id, which leads to the consumer completion, but throwing an exception at the save-context stage. In this case, only the not-typed consumer receives the notification. (Bug).
