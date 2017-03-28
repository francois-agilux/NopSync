
using BasicNopSync.OData;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Tests
{
    [TestClass]
    public class UrlBuilderTests
    {
        [TestMethod]
        public void SelectTest()
        {
            string expectedResultSimple = "Product?$select=Id";
            string expectedResultMultiple = "Product?$select=Id,Name";

            string selectResultId = "Product(15)?$select=Name";
            
            Assert.AreEqual<string>(expectedResultSimple, new UrlBuilder("Product").Select("Id").BuildQuery());

            Assert.AreEqual<string>(expectedResultMultiple, new UrlBuilder("Product").Select("Id","Name").BuildQuery());

            Assert.AreEqual<string>(selectResultId, new UrlBuilder("Product").Id(15).Select("Name").BuildQuery());

        }

        [TestMethod]
        public void FilterTest()
        {
            string expectedResultInt = "Product?$filter=Id+eq+1";
            string expectedResultString = "Product?$filter=Name+eq+'Name'";
            string expectedResultStartsWith = "Product?$filter=startswith(Name,'Name')";
            string expectedResultBool = "Product?$filter=Bool+ne+true";
            string expectedResultCombineAnd = "Product?$filter=Id+eq+1+and+Name+eq+'Name'";
            string expectedResultCombineOr = "Product?$filter=Id+eq+1+or+Name+eq+'Name'";

            
            Assert.AreEqual<string>(expectedResultInt, new UrlBuilder("Product").FilterEq("Id",1).BuildQuery());
                        
            Assert.AreEqual<string>(expectedResultString, new UrlBuilder("Product").FilterEq("Name","Name").BuildQuery());

            
            Assert.AreEqual<string>(expectedResultStartsWith, new UrlBuilder("Product").FilterStartsWith("Name","Name").BuildQuery());

            
            Assert.AreEqual<string>(expectedResultBool, new UrlBuilder("Product").FilterNe("Bool", true).BuildQuery());

            
            Assert.AreEqual<string>(expectedResultCombineAnd, new UrlBuilder("Product").FilterEq("Id", 1).And().FilterEq("Name","Name").BuildQuery());

            
            Assert.AreEqual<string>(expectedResultCombineOr, new UrlBuilder("Product").FilterEq("Id", 1).FilterEq("Name", "Name").Or().BuildQuery());

        }

        [TestMethod]
        public void ExpandTest()
        {
            string expectedResultSimple = "Product?$expand=Addresses";            
            
            Assert.AreEqual<string>(expectedResultSimple, new UrlBuilder("Product").Expand("Addresses").BuildQuery());            

        }

        [TestMethod]
        public void RefTest()
        {
            string expectedResultSimple = "Product(1)/Addresses/$ref";            

            UrlBuilder u = new UrlBuilder("Product");

            Assert.AreEqual<string>(expectedResultSimple, u.BuildQueryRef(1,"Addresses"));

        }

        [TestMethod]
        public void CombineTest()
        {
            string expectedResult = "Product?$select=Id,Sku&$filter=Id+gt+5+and+Name+eq+'Name'";
            
            Assert.AreEqual<string>(expectedResult, new UrlBuilder("Product").FilterGt("Id",5).And().FilterEq("Name","Name").Select("Id","Sku").BuildQuery());
            

        }

    }
}
