using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Collections.Generic;
using InventoryModels;
using InventoryTesting;

namespace InventorySimulation
{
    /// <summary>
    /// Handles reading and writing from test case files
    /// </summary>
    static class TestCaseManager
    {
        class Header
        {
            public string Name;
            public bool IsSingleLine = true, IsOneArgument = true;
            public Action<SimulationSystem, string[]> Input;
            public Action<SimulationSystem, StringBuilder> Output;
        }
        static readonly List<Header> Headers = new List<Header>()
        {
            new Header()
            {
                Name = "OrderUpTo",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.OrderUpTo = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.OrderUpTo);
                }
            },
            new Header()
            {
                Name = "ReviewPeriod",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.ReviewPeriod = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.ReviewPeriod);
                }
            },
            new Header()
            {
                Name = "StartInventoryQuantity",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.StartInventoryQuantity = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.StartInventoryQuantity);
                }
            },
            new Header()
            {
                Name = "StartLeadDays",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.StartLeadDays = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.StartLeadDays);
                }
            },
            new Header()
            {
                Name = "StartOrderQuantity",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.StartOrderQuantity = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.StartOrderQuantity);
                }
            },
            new Header()
            {
                Name = "NumberOfDays",
                Input = (SimulationSystem s, string[] val) =>
                {
                    s.NumberOfDays = int.Parse(val[0]);
                },
                Output = (SimulationSystem s, StringBuilder b) =>
                {
                    b.Append(s.NumberOfDays);
                }
            },
            new Header()
            {
                Name = "DemandDistribution",
                IsOneArgument = false,
                IsSingleLine = false,
                Input = (SimulationSystem s, string[] val) =>
                {
                    foreach(string str in val)
                    {
                        string[] arr = str.Split(',');
                        s.DemandDistribution.Add(new Distribution()
                        {
                            Value = int.Parse(arr[0]),
                            Probability = decimal.Parse(arr[1])
                        });
                    }
                }
            },
            new Header()
            {
                Name = "LeadDaysDistribution",
                IsOneArgument = false,
                IsSingleLine = false,
                Input = (SimulationSystem s, string[] val) =>
                {
                    foreach(string str in val)
                    {
                        string[] arr = str.Split(',');
                        s.LeadDaysDistribution.Add(new Distribution()
                        {
                            Value = int.Parse(arr[0]),
                            Probability = decimal.Parse(arr[1])
                        });
                    }
                }
            }
        };
        /// <summary>
        /// Extracts simulation system out of a file
        /// </summary>
        /// <param name="path">path to simulation system file</param>
        static public SimulationSystem FromFile(string path)
        {
            StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read));
            SimulationSystem system = new SimulationSystem();
            while (reader.Peek() != -1)
            {
                string header = reader.ReadLine().Trim();
                bool HeaderFound = false;
                foreach (Header h in Headers)
                {
                    if (h.Name == header)
                    {
                        HeaderFound = true;
                        List<string> lines = new List<string>();
                        string s;
                        while (!string.IsNullOrWhiteSpace(s = reader.ReadLine()))
                        {
                            s.Trim();
                            if (s.Split(',').Length > 1 && h.IsOneArgument)
                                throw new FormatException("Header \"" + header + "\" expects only a single argument per line, receieved " + lines.Count + " arguments");
                            lines.Add(s);
                        }
                        if (h.IsSingleLine && lines.Count > 1)
                            throw new FormatException("Header \"" + header + "\" expects only a single line, receieved " + lines.Count + " lines");
                        h.Input(system, lines.ToArray());
                        break;
                    }
                }
                if (!HeaderFound)
                    throw new ArgumentException("Header \"" + header + "\" is not defined");
            }
            reader.Close();
            return system;
        }
        /// <summary>
        /// Writes the simulation system to a string
        /// </summary>
        /// <param name="system">The simulation system to be written</param>
        static public string ToString(SimulationSystem system)
        {
            StringBuilder builder = new StringBuilder();
            foreach (Header header in Headers)
            {
                builder.AppendLine(header.Name);
                header.Output(system, builder);
                builder.AppendLine();
            }
            return builder.ToString().Trim();
        }
        /// <summary>
        /// Writes a simulation system to a file. if the file exists it will be overwritten, otherwise it will be created
        /// </summary>
        /// <param name="system">The simulation system to be written</param>
        /// <param name="path">path to file</param>
        static public void ToFile(SimulationSystem system, string path)
        {
            FileStream file = new FileStream(path, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(file);
            writer.Write(ToString(system));
            writer.Close();
        }
        /// <summary>
        /// Runs all test cases in TestCases folder
        /// </summary>
        /// <returns>Result of running all of the testcases</returns>
        static public string RunAllTestCases()
        {
            string path = Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory().ToString()).ToString()) + "\\TestCases\\";
            StringBuilder builder = new StringBuilder();
            Stopwatch watch = new Stopwatch();
            int i = 1;
            foreach (string file in Directory.EnumerateFiles(path))
            {
                string fileName = file.Substring(file.LastIndexOf('\\') + 1);
                if (fileName.Contains("TestCase"))
                {
                    builder.AppendLine("---TestCase #" + i++);
                    SimulationSystem system = FromFile(file);
                    watch.Restart();
                    Simulator.StartSimulation(system);
                    watch.Stop();
                    builder.AppendLine(TestingManager.Test(system, fileName)
                                     + "\nTime = " + watch.ElapsedMilliseconds + "ms");
                }
            }
            return builder.ToString().Trim();
        }
    }
}