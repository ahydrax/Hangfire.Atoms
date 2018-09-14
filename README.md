# Hangfire.Atoms
[![NuGet](https://img.shields.io/nuget/v/Hangfire.Atoms.svg)](https://www.nuget.org/packages/Hangfire.Atoms/)

Execute multiple jobs as a single atomic job.

## Requirements
* The storage you chosen must implement `JobStorageConnection` & `JobStorageTransaction`
* NET Standard 2.0 compatible project

## Setup
Install a package from Nuget. Then configure your server and dashboard like this:

```csharp
services.AddHangfire(configuration =>
    {
        configuration.UseStorage(yourStorage);
        configuration.UseAtoms();
    });
```
Or this:

```csharp
GlobalConfiguration.UseStorage(yourStorage);
GlobalConfiguration.UseAtoms();
```

You **must** setup Hangfire storage before calling `UseAtoms();`.

## Usage
Additional extension methods are added for `IBackgroundJobClient`.

```csharp
var client = new BackgroundJobClient();

// Enqueue
var atomId = client.Enqueue("atom-1", builder =>
    {
        for (var i = 0; i < 50; i++)
        {
            builder.Enqueue(() => DoWork());
        }
    });

// Continuations
var job1 = client.Enqueue(() => DoPrepare());
var atomId = client.ContinueWith(job1, "atom-2", builder =>
    {
        for (var i = 0; i < 50; i++)
        {
            builder.Enqueue(() => DoWork());
        }
    });

// Scheduling
var atomId = client.Schedule("atom-3", TimeSpan.FromSeconds(3), builder =>
    {
        for (var i = 0; i < 50; i++)
        {
            builder.Enqueue(() => DoWork());
        }
    });

// Atoms can be used as a common job which means you can continue them
client.ContinueWith(atomId, () => Done());
```

## Triggers
Triggers are event-like primitives. You can subscribe to it and set it manually whenever you need it.

```csharp
// Triggers
var triggerJobId = client.OnTriggerSet("trigger-1");
client.ContinueWith(triggerJobId, () => DoWork());

// Set trigger manually when you need it
client.Schedule(() => SetTrigger("trigger-1"), TimeSpan.FromSeconds(10));
```

## License
Authored by: Viktor Svyatokha (ahydrax)

This project is under MIT license. You can obtain the license copy [here](https://github.com/ahydrax/Hangfire.Atoms/blob/master/LICENSE).

This work is based on the work of Sergey Odinokov, author of Hangfire. <http://hangfire.io/>