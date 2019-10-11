using LOIN.Context;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LOIN.Tests
{
    [TestClass]
    public class ReportingTests
    {
        [TestMethod]
        public void ReportingTest()
        {
            const string file = @"Files\sample_20190809_1625.ifc";
            using (var model = Model.Open(file))
            {
                foreach (var item in model.BreakdownStructure)
                {
                    foreach (var milestone in model.Milestones)
                    {
                        foreach (var actor in model.Actors)
                        {
                            foreach (var reason in model.Reasons)
                            {
                                var requirementSets = 
                                    model.GetRequirements(item, milestone, actor, reason)
                                    .SelectMany(r => r.Requirements)
                                    .ToList();
                                if (!requirementSets.Any())
                                    continue;

                                Console.WriteLine();
                                Console.WriteLine("CONTEXT:");
                                Console.WriteLine($"  Breakdown Item: {item.Name}");
                                Console.WriteLine($"  Milestone: {milestone.Name}");
                                Console.WriteLine($"  Actor: {actor.Name}");
                                Console.WriteLine($"  Reason: {reason.Name}");

                                foreach (var requirementSet in requirementSets.Where(r => r.HasPropertyTemplates.Any()))
                                {
                                    Console.WriteLine($"    Requirement set: {requirementSet.Name}");
                                    foreach (var requirement in requirementSet.HasPropertyTemplates)
                                    {
                                        Console.WriteLine($"    Requirement: {requirement.Name} ({requirement.Description})");
                                    }
                                }
                            }
                        }
                    }
                }
            }

        }
    }
}
