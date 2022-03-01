using Microsoft.VisualStudio.TestTools.UnitTesting;
using PonzianiSwissLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PonzianiSwissTest
{
    [TestClass]
    public class TestPlayerDatabase
    {
        [ClassInitialize()]
        public static void ClassInit()
        {
            using var db = new FidePlayerContext();
            Assert.IsNotNull(db);
            testdb = db.DbPath.Replace(".db", "_test.db");
            if (!File.Exists(testdb))
                File.Copy(db.DbPath, testdb);
        }

        private static string testdb = string.Empty;

        [TestMethod]
        public void TestAdminData()
        {
            using var db = new FidePlayerContext(Path.GetFileName(testdb));
            var date = db.GetUpdateDateAsync().Result;
            Assert.IsNotNull(date);
            if (db.SetUpdateDateAsync().Result)
            {
                date = db.GetUpdateDateAsync().Result;
                Assert.IsTrue((DateTime.UtcNow - date) < TimeSpan.FromMinutes(1));
            }
            else Assert.Fail();
        }

        [TestMethod]
        public void TestUpdateDatabase()
        {
            using var db = new FidePlayerContext(Path.GetFileName(testdb));
            int count = db.UpdateAsync().Result;
            Console.WriteLine($"{count} records changed!");
            Assert.IsTrue(count >= 1);
        }
    }

}
