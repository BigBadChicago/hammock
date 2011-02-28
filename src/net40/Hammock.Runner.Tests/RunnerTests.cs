using System;
using System.Diagnostics;
using NUnit.Framework;

namespace Hammock.Runner.Tests
{
    [TestFixture]
    public class RunnerTests
    {
        private const string Location = "http://hammockrest.com.s3.amazonaws.com/tests/";

        [Test]
        public void Can_get_usage_when_no_parameters_passed()
        {
            var output = WithHammock("");
            Assert.IsNotNullOrEmpty(output);
            Assert.IsTrue(output.StartsWith("Hammock"));
        }

        [Test]
        public void Can_get_url()
        {
            var output = WithHammock(string.Concat(Location, "hello.html"));
            Assert.IsNotNullOrEmpty(output);
            Console.WriteLine(output);
        }

        private static string WithHammock(string command)
        {
            var process = new Process
                              {
                                  StartInfo =
                                      {
                                          UseShellExecute = false,
                                          RedirectStandardOutput = true,
                                          FileName = "hammock.exe",
                                          Arguments = command
                                      }
                              };

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();
            return output;
        }
    }
}