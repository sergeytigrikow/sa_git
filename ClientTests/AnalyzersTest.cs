using System;
using System.Text.RegularExpressions;
using CommonClassLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServerClasses.Analyzers;

namespace ClientTests
{
    [TestClass]
    public class AnalyzersTest
    {
        [TestMethod]
        public void CYTest()
        {
            var cy = new CYAnalyzer(RequestType.CY) {SiteUrl = "tut.by"};
            cy.Proceed();
            Assert.AreEqual(cy.Result, "0");
        }

        [TestMethod]
        public void PRTest()
        {
            var pr = new PRAnalyzer(RequestType.CY) { SiteUrl = "tut.by" };
            pr.Proceed();
            Assert.AreEqual(pr.Result, "450");
        }

        [TestMethod]
        public void LinksTest()
        {
            var links = new LinksAnalyzer(RequestType.CY) { SiteUrl = "tut.by" };
            links.Proceed();
            Assert.AreNotEqual(links.Result, String.Empty);
        }

        [TestMethod]
        public void CustomersTest()
        {
            var cust = new CustomersAnalyzer(RequestType.CY) { SiteUrl = "tut.by" };
            cust.Proceed();
            Assert.IsNotNull(cust.Result);
        }
    }
}
