# What is Suspension

__Suspension__ is a tool for generating suspendable functions for C# - coroutines. If you familiar with `yield return` or coroutine concept you will quick understand what it is.

Coroutine is pull based piece of code. So you may ask it to run until next suspension point.

__Suspension__ provides two kinds of coroutines:

1. Parameterless - coroutine requires no parameters to continue (it's very like regular `yield return`)
1. Parameterized - coroutine requires a value to continue (it's very like [sending values to the generator in JavaScript](https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Global_Objects/Generator/next#Sending_values_to_the_generator))

# Show me the code

Lets say you have following `WriteToConsole` method

```csharp
// partial modifier is needed for source generator
partial class HelloWorld
{ 
    [Suspendable]
    public string WriteToConsole()
    {
        Console.WriteLine("Hello");

        // this is suspension point named "Middle"
        // the name of suspension point must be unique for this method
        Suspension.Flow.Suspend("Middle");
        Console.WriteLine("World!");
        return "We made it!";
    }
}
```

with __Suspension__ you can run it like this:

```csharp
HelloWorld.Coroutines.WriteToConsole startCoroutine = new HelloWorld.Coroutines.WriteToConsole.Start();
// pitfall coroutine.Run() not changes coroutine, but returns a new one, so you need to store it
HelloWorld.Coroutines.WriteToConsole middleCoroutine = startCoroutine.Run(); // prints "Hello"
HelloWorld.Coroutines.WriteToConsole finishCoroutine = middleCoroutine.Run(); // prints "World!"
Console.WriteLine(finishCoroutine.Completed); // true
Console.WriteLine(finishCoroutine.Result); // "We made it!"
```

so regular usage looks like:

```csharp
while (!coroutine.Completed)
{
    if (WeDontNeedItAnymore())
    {
        // we just throw away the partially completed coroutine
        return;
    }

    coroutine = coroutine.Run();
}

DoSomethingCoolWith(coroutine.Result);
```

# How are coroutines different from `yield return`?

- Coroutine allows to inspect it's state
```csharp
partial class Countdown
{ 
    [Suspendable]
    public void WriteToConsole(int count)
    {
        while (count > 0)
        {
            Console.WriteLine(count);
            count--;
            Suspension.Flow.Suspend("Cycle");
        }
    }
}

class Print : Countdown.Coroutines.WriteToConsole.Visitor<string>
{
    public override string VisitStart(int count) => $"Start: {count}";
    public override string VisitCycle(int count) => $"Cycle: {count}";
    public override string VisitFinish() => "Finish";
}

var startCoroutine = new Countdown.Coroutines.WriteToConsole.Start(10);
var print = new Print();
var startPrint = startCoroutine.Accept(print); // Start: 10
var cycleCoroutine = startCoroutine.Run();
var cyclePrint = cycleCoroutine.Accept(print); // Cycle: 9
```
- Coroutine allows to reset method execution to any suspension point just storing corresponding coroutine instance
- `Coroutine.Run()` returns new instance each time, so it's a bit more expensive than `yield return`
- Coroutine may end up with a single result
- Coroutine may consume values from outside (in fact, you can fork single coroutine passing different values)
```csharp
partial class PartialHelloWorld
{ 
    [Suspendable]
    public void WriteToConsole()
    {
        Console.WriteLine("Hello");
        var needWorld = Suspension.Flow<bool>.Wait("NeedWorld");
        if (needWorld)
        {
            Console.WriteLine("World!");
        }
    }
}

var startCoroutine = new PartialHelloWorld.Coroutines.WriteToConsole.Start();
var needWorld = startCoroutine.Run(); // prints "Hello"
middleCoroutine.Run(false); // prints nothing
middleCoroutine.Run(true); // prints "World!"
```
- Coroutine does not implements `IDisposable` interface, so there is no way to `Dispose` acquired by coroutine resources. Thus, it is highly recommended to avoid `using` statements that cross suspension point. (todo make example)

# Guarantees and invariants

- Each coroutine work like that

| `Coroutine.Completed` | `Coroutine.Result`                 | `Coroutine.Run()`                      |
|-----------------------|------------------------------------|----------------------------------------|
| `true`                | returns result                     | throws `InvalidOperationException`     |
| `false`               | throws `InvalidOperationException` | continues coroutine and give a new one |

- Coroutine is immutable even if original method modifies variables and fields, so once you get a particular coroutine instance you may run it as many times as you wish, even in parallel


# Current state and plans

This library is under developement.

Roadmap is:
- implement parameterless coroutines
- implement visitor for state inspecting
- wrap it in source generator
- implement nice `Coroutine.ToString()` method that shows where execution suspended and values of variables
- implement source mapping
- implement parameterized coroutines
- add async coroutines (which has `RunAsync()` instead of `Run()`)
- implement parsing coroutines which useful in parsing of partially arrived data (consume array of T (usually bytes) and return array of TResult that currently parsed)