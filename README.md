# Run-This?

## Premises

1. Keep an `Actor` **mono-threaded** by means of a `ProxyInvoker`
2. Optimize for `ValueTask` calls, by making callers wait for an `ExecutionSlot`, and only then execute the call. This means that `OneWay` calls are defined by the user on a per call basis, just like on traditional .NET TPL.

No `2` seems to be achievable by simply not awaiting any calls, and just letting the .NET Threadpool do its thing. Therefore, no *special dispatcher (work consumer/scheduler)* needed. 