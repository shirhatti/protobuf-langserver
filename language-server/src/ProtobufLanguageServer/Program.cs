using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Server;
using System;
using System.Threading.Tasks;

namespace ProtobufLanguageServer
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var server = await LanguageServer.From(options =>
                options
                    .WithInput(Console.OpenStandardInput())
                    .WithOutput(Console.OpenStandardOutput())
                    .WithLoggerFactory(new LoggerFactory())
                    .AddDefaultLoggingProvider()
                    .WithMinimumLogLevel(LogLevel.Trace)

                    // We're adding a new endpoint that will handle a specific set of language server features.
                    .WithHandler<TextDocumentSynchronizationEndpoint>()
                    .WithServices(services =>
                    {
                        // Register any custom services here
                    }));

            await server.WaitForExit;
        }
    }
}
