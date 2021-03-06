using NUnit.Framework;
using OpenWrap.Testing;

namespace Tests.Commands.command_line_runner
{
    public class missing_optional_named_input : contexts.command_line_runner
    {
        public missing_optional_named_input()
        {
            given_command("get", "help", command => command);
            when_executing(string.Empty);
        }

        [Test]
        public void command_is_executed()
        {
            CommandExecuted.ShouldBeTrue();
        }

        [Test]
        public void input_not_assigned()
        {
            Input("command").ShouldBeEmpty();
        }
    }
}