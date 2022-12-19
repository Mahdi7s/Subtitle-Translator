using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SubtitleTranslator;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var path = @"C:\Users\Mahdi7S\Documents\New folder\SubtitleTranslator.Application.exe";
            PluginSettings.Save(new PluginSettings { ProgramPath = path });

            //Process.Start(path);

            var setts = PluginSettings.Load();

            Assert.AreEqual(setts.ProgramPath, path);
        }
    }
}
