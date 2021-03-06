﻿using NUnit.Framework;
using Tests.Build.build_instruction_emitter.contexts;

namespace Tests.Build.build_instruction_emitter
{
    public class pdb_for_referenced_assembly : msbuild_emitter
    {
        public pdb_for_referenced_assembly()
        {
            given_export_name("bin-net35");
            given_output("sauron.dll");
            given_output("sauron.pdb");
            when_generating_instructions();
        }

        [Test]
        public void pdb_is_included()
        {
            should_have_file("bin-net35", "sauron.pdb");
        }
    }
}