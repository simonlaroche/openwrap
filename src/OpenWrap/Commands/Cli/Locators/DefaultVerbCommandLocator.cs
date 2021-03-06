﻿using System;
using System.Linq;
using OpenWrap.Collections;

namespace OpenWrap.Commands.Cli.Locators
{
    public class DefaultVerbCommandLocator : ICommandLocator
    {
        readonly ICommandRepository _commandRepository;

        public DefaultVerbCommandLocator(ICommandRepository commandRepository)
        {
            _commandRepository = commandRepository;
        }

        public ICommandDescriptor Execute(ref string input)
        {
            if (string.IsNullOrEmpty(input)) return null;
            var firstToken = input.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (string.IsNullOrEmpty(firstToken)) return null;

            var noun = firstToken.SelectHumps(_commandRepository.Nouns).OneOrDefault();

            if (noun == null) return null;

            var command = _commandRepository.Distinct(CommandDescriptorComparer.VerbNoun).Where(x => x != null && x.Noun.EqualsNoCase(noun) && x.IsDefault).OneOrDefault();
            if (command != null)
                input = input.TrimStart().Substring(noun.Length).TrimStart();

            return command;
        }
    }
}