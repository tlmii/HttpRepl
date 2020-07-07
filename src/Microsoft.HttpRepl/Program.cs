// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.FileSystem;
using Microsoft.HttpRepl.Preferences;
using Microsoft.HttpRepl.UserProfile;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.ConsoleHandling;

namespace Microsoft.HttpRepl
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new Program().Start(args);
        }

        public async Task Start(string[] args, IConsoleManager consoleManager = null, IPreferences preferences = null)
        {
            args = args ?? throw new ArgumentNullException(nameof(args));

            RegisterEncodingProviders();
            ComposeDependencies(ref consoleManager, ref preferences, out HttpState state, out Shell shell);

            if (Console.IsOutputRedirected && !consoleManager.AllowOutputRedirection)
            {
                Reporter.Error.WriteLine(Resources.Strings.Error_OutputRedirected.SetColor(preferences.GetColorValue(WellKnownPreference.ErrorColor)));
                return;
            }

            using (CancellationTokenSource source = new CancellationTokenSource())
            {
                var runCommand = new Command("run", "Execute commands in script file")
                {
                    new Argument<FileInfo>("fileName", "Script file name")
                };

                runCommand.Handler = CommandHandler.Create<FileInfo>(async (fileName) => await HandleRunCommand(shell, fileName));

                var rootCommand = new RootCommand()
                {
                    runCommand,
                    new Argument<string>("rootAddress", "Root address to use for the connect command") { Arity = ArgumentArity.ZeroOrOne, IsHidden = true },
                    new Option<string>(new[] { "--base", "-b" }, "Base address to use for the connect command") { IsHidden = true },
                    new Option<string>(new[] { "--openapi", "-o" }, "OpenAPI Description address to use for the connect command") { IsHidden = true }
                };

                rootCommand.Handler = CommandHandler.Create<string, string, string>(async (rootAddress, @base, openApi) =>
                {
                    await HandleConnectCommand(shell, source, rootAddress, @base, openApi);
                });

                shell.ShellState.ConsoleManager.AddBreakHandler(() => source.Cancel());

                var commandLineBuilder = new CommandLineBuilder(rootCommand);
                await commandLineBuilder.UseDefaults()
                                        .UseHelpBuilder(context => new CustomHelpBuilder(context.Console, shell, preferences, state))
                                        .Build()
                                        .InvokeAsync(args);
            }
        }

        private async Task HandleRunCommand(Shell shell, FileInfo fileInfo)
        {
            shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
            shell.ShellState.InputManager.SetInput(shell.ShellState, string.Join(' ', new[] { "run", fileInfo.FullName }));
            await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);
        }

        private async Task HandleConnectCommand(Shell shell, CancellationTokenSource source, string rootAddress, string baseAddress, string openApiAddress)
        {
            List<string> connectArgs = new List<string>();
            if (!string.IsNullOrWhiteSpace(rootAddress))
            {
                connectArgs.Add(rootAddress);
            }
            if (!string.IsNullOrWhiteSpace(baseAddress))
            {
                connectArgs.Add("--base");
                connectArgs.Add(baseAddress);
            }
            if (!string.IsNullOrWhiteSpace(openApiAddress))
            {
                connectArgs.Add("--openapi");
                connectArgs.Add(openApiAddress);
            }

            string combinedArgs = string.Join(' ', connectArgs);

            shell.ShellState.CommandDispatcher.OnReady(shell.ShellState);
            shell.ShellState.InputManager.SetInput(shell.ShellState, $"connect {combinedArgs}");
            await shell.ShellState.CommandDispatcher.ExecuteCommandAsync(shell.ShellState, CancellationToken.None).ConfigureAwait(false);

            await shell.RunAsync(source.Token).ConfigureAwait(false);
        }

        private static void ComposeDependencies(ref IConsoleManager consoleManager, ref IPreferences preferences, out HttpState state, out Shell shell)
        {
            consoleManager = consoleManager ?? new ConsoleManager();
            IFileSystem fileSystem = new RealFileSystem();
            preferences = preferences ?? new UserFolderPreferences(fileSystem, new UserProfileDirectoryProvider(), CreateDefaultPreferences());
            var httpClient = GetHttpClientWithPreferences(preferences);
            state = new HttpState(fileSystem, preferences, httpClient);

            var dispatcher = DefaultCommandDispatcher.Create(state.GetPrompt, state);
            dispatcher.AddCommand(new ChangeDirectoryCommand());
            dispatcher.AddCommand(new ClearCommand());
            dispatcher.AddCommand(new ConnectCommand(preferences));
            dispatcher.AddCommand(new DeleteCommand(fileSystem, preferences));
            dispatcher.AddCommand(new EchoCommand());
            dispatcher.AddCommand(new ExitCommand());
            dispatcher.AddCommand(new HeadCommand(fileSystem, preferences));
            dispatcher.AddCommand(new HelpCommand(preferences));
            dispatcher.AddCommand(new GetCommand(fileSystem, preferences));
            dispatcher.AddCommand(new ListCommand(preferences));
            dispatcher.AddCommand(new OptionsCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PatchCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PrefCommand(preferences));
            dispatcher.AddCommand(new PostCommand(fileSystem, preferences));
            dispatcher.AddCommand(new PutCommand(fileSystem, preferences));
            dispatcher.AddCommand(new RunCommand(fileSystem));
            dispatcher.AddCommand(new SetBaseCommand());
            dispatcher.AddCommand(new SetHeaderCommand());
            dispatcher.AddCommand(new UICommand(new UriLauncher(), preferences));

            shell = new Shell(dispatcher, consoleManager: consoleManager);
        }

        internal static Dictionary<string, string> CreateDefaultPreferences()
        {
            return new Dictionary<string, string>
            {
                { WellKnownPreference.ProtocolColor, "BoldGreen" },
                { WellKnownPreference.StatusColor, "BoldYellow" },

                { WellKnownPreference.JsonArrayBraceColor, "BoldCyan" },
                { WellKnownPreference.JsonCommaColor, "BoldYellow" },
                { WellKnownPreference.JsonNameColor, "BoldMagenta" },
                { WellKnownPreference.JsonNameSeparatorColor, "BoldWhite" },
                { WellKnownPreference.JsonObjectBraceColor, "Cyan" },
                { WellKnownPreference.JsonColor, "Green" }
            };
        }

        private static HttpClient GetHttpClientWithPreferences(IPreferences preferences)
        {
            if (preferences.GetBoolValue(WellKnownPreference.UseDefaultCredentials))
            {
#pragma warning disable CA2000 // Dispose objects before losing scope
                return new HttpClient(new HttpClientHandler { UseDefaultCredentials = true });
#pragma warning restore CA2000 // Dispose objects before losing scope
            }

            return new HttpClient();
        }

        private static void RegisterEncodingProviders()
        {
            // Adds Windows-1252, among others
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }
    }
}
