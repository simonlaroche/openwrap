﻿using NUnit.Framework;
using OpenWrap.Repositories;
using OpenWrap.Testing;

namespace Tests.Commands.add_remote.priority
{
    public class adding_new_remote_empty_config : contexts.add_remote
    {
        public adding_new_remote_empty_config()
        {
            given_remote_factory(input => new InMemoryRepository(input));
            when_executing_command("iron-hills http://lotr.org/iron-hills");
        }

        [Test]
        public void initial_priority_is_1()
        {
            ConfiguredRemotes["iron-hills"].Priority.ShouldBe(1);
        }
    }
}