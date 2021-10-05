using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using EventStore.Client;
using Newtonsoft.Json;
using Spectre.Console;

namespace QueueingSystemEventWriter
{
    class Program
    {
        private static readonly EventStoreClient EventStoreClient;
        private static StreamRevision ExpectedStreamRevision = StreamRevision.None;
        
        static Program()
        {
            var settings = EventStoreClientSettings.Create("esdb://localhost:2111,localhost:2112,localhost:2113?tls=true&tlsVerifyCert=false");
            settings.DefaultCredentials = new UserCredentials("admin", "changeit");
            EventStoreClient = new EventStoreClient(settings);
        }
        
        [SuppressMessage("ReSharper.DPA", "DPA0001: Memory allocation issues")]
        static void Main(string[] args)
        {
            AnsiConsole.Progress()
                .Columns(new ProgressColumn[] 
                {
                    new ProgressBarColumn(),
                    new PercentageColumn(),
                    new RemainingTimeColumn(),
                    new TransferSpeedColumn(),
                    new SpinnerColumn(),
                    new ElapsedTimeColumn()
                })
                .Start(ctx => 
                {
                    var progressTask = ctx.AddTask("[green] Generated events [/]", new ProgressTaskSettings {MaxValue = 10000000});

                    var queueingSystemGenerator = new QueueingSystemGenerator(1, 2, 1, DateTime.Now);

                    var enumerable = queueingSystemGenerator.Generate();

                    while (!ctx.IsFinished)
                    {
                        progressTask.Increment(StoreEvents(enumerable.Take(5000)));
                    }
                });
        }

        private static long StoreEvents(IEnumerable<object> batch)
        {
            var writeResult = EventStoreClient.AppendToStreamAsync(
                "AllQueues",
                ExpectedStreamRevision,
                batch.Select(e => new EventData(
                    Uuid.NewUuid(),
                    e.GetType().Name,
                    Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(e))))).Result;

            var producedEvents = writeResult.NextExpectedStreamRevision.ToInt64() - ExpectedStreamRevision.ToInt64();
            ExpectedStreamRevision = writeResult.NextExpectedStreamRevision;
            return producedEvents;
        }
    }
}