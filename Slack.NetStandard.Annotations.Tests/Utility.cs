﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Slack.NetStandard.Annotations.Tests
{
    internal class Utility
    {
        public static Task Verify(string sampleCode)
        {
            var tree = CSharpSyntaxTree.ParseText(sampleCode);

            var compilation = CSharpCompilation.Create("Tests", new[] { tree });

            var generator = new SlackAppGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);
            return Verifier.Verify(driver).UseDirectory("Snapshots");
        }

        public static bool HasSource(string sampleCode, string name)
        {
            var tree = CSharpSyntaxTree.ParseText(sampleCode);

            var compilation = CSharpCompilation.Create("Tests", new[] { tree });

            var generator = new SlackAppGenerator();
            GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
            driver = driver.RunGenerators(compilation);
            return driver.GetRunResult().Results.SelectMany(r => r.GeneratedSources).Any(s => s.HintName == name);
        }
    }
}
