# Run-This?

## Premises

1. Keep an `Actor` **mono-threaded** by means of a mailbox
2. Optimize for `ValueTask` calls, by tiing the start of a `ValueTask` call to the next idleness of the `Actor`. This means that `OneWay` calls are defined by the user on a per call basis, the same behavior we have on the traditional .NET TPL.
2. Given a mono-threaded **work source**, split the work per `actor` in a parallel fashion, maximizing CPU usage.

No `2` seems to be achievable by simply not awaiting any calls, and just letting the .NET Threadpool do its thing. Therefore, no *special dispatcher* needed. 