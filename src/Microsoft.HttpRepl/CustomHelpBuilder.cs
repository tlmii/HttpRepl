using System.CommandLine;
using System.CommandLine.Help;
using Microsoft.HttpRepl.Commands;
using Microsoft.HttpRepl.Preferences;
using Microsoft.Repl;
using Microsoft.Repl.Commanding;
using Microsoft.Repl.Parsing;

namespace Microsoft.HttpRepl
{
    internal class CustomHelpBuilder : HelpBuilder
    {
        private readonly Shell _shell;
        private readonly IPreferences _preferences;
        private readonly HttpState _state;

        public CustomHelpBuilder(IConsole console, Shell shell, IPreferences preferences, HttpState state) : base(console)
        {
            _shell = shell;
            _preferences = preferences;
            _state = state;
        }

        public override void Write(ICommand command)
        {
            base.Write(command);
            _shell.ShellState.ConsoleManager.WriteLine();
            _shell.ShellState.ConsoleManager.WriteLine(Resources.Strings.Help_REPLCommands);
            new HelpCommand(_preferences).CoreGetHelp(_shell.ShellState, (ICommandDispatcher<HttpState, ICoreParseResult>)_shell.ShellState.CommandDispatcher, _state);
        }

    }
}
