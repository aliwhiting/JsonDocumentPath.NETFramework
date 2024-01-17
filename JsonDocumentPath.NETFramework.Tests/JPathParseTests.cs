using System.Text.Json;
using JsonDocumentPath.NETFramework.Filters;
using JsonDocumentPath.NETFramework.Tests.Asserts;

namespace JsonDocumentPath.NETFramework.Tests
{
    [TestClass]
    public class JsonDocumentPathParseTests
    {
        [TestMethod]
        public void BooleanQuery_TwoValues()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(1 > 2)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(1, ((JsonElement)booleanExpression.Left).GetInt32());
            Assert.AreEqual(2, ((JsonElement)booleanExpression.Right).GetInt32());
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [TestMethod]
        public void BooleanQuery_TwoPaths()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.price > @.max_price)]");
            Assert.AreEqual(1, path.Filters.Count);
            BooleanQueryExpression booleanExpression = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> leftPaths = (List<PathFilter>)booleanExpression.Left;
            List<PathFilter> rightPaths = (List<PathFilter>)booleanExpression.Right;

            Assert.AreEqual("price", ((FieldFilter)leftPaths[0]).Name);
            Assert.AreEqual("max_price", ((FieldFilter)rightPaths[0]).Name);
            Assert.AreEqual(QueryOperator.GreaterThan, booleanExpression.Operator);
        }

        [TestMethod]
        public void SingleProperty()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SingleQuotedProperty()
        {
            JsonDocumentPath path = new JsonDocumentPath("['Blah']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SingleQuotedPropertyWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("[  'Blah'  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SingleQuotedPropertyWithDots()
        {
            JsonDocumentPath path = new JsonDocumentPath("['Blah.Ha']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah.Ha", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SingleQuotedPropertyWithBrackets()
        {
            JsonDocumentPath path = new JsonDocumentPath("['[*]']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("[*]", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SinglePropertyWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SinglePropertyWithRootWithStartAndEndWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath(" $.Blah ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void RootWithBadWhitespace()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("$ .Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [TestMethod]
        public void NoFieldNameAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("$.Blah."); }, @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void RootWithBadWhitespace2()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("$. Blah"); }, @"Unexpected character while parsing path:  ");
        }

        [TestMethod]
        public void WildcardPropertyWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void WildcardArrayWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void RootArrayNoDot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$[1]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void WildcardArray()
        {
            JsonDocumentPath path = new JsonDocumentPath("[*]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void WildcardArrayWithProperty()
        {
            JsonDocumentPath path = new JsonDocumentPath("[ * ].derp");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual(null, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual("derp", ((FieldFilter)path.Filters[1]).Name);
        }

        [TestMethod]
        public void QuotedWildcardPropertyWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.['*']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("*", ((FieldFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void SingleScanWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$..Blah");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual("Blah", ((ScanFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void QueryTrue()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.elements[?(true)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, ((QueryFilter)path.Filters[1]).Expression.Operator);
        }

        [TestMethod]
        public void ScanQuery()
        {
            JsonDocumentPath path = new JsonDocumentPath("$.elements..[?(@.id=='AAA')]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("elements", ((FieldFilter)path.Filters[0]).Name);

            BooleanQueryExpression expression = (BooleanQueryExpression)((QueryScanFilter)path.Filters[1]).Expression;

            List<PathFilter> paths = (List<PathFilter>)expression.Left;

            Assert.IsInstanceOfType<FieldFilter>(paths[0]);
        }

        [TestMethod]
        public void WildcardScanWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("$..*");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void WildcardScanWithRootWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("$..* ");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ScanFilter)path.Filters[0]).Name);
        }

        [TestMethod]
        public void TwoProperties()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah.Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((FieldFilter)path.Filters[1]).Name);
        }

        [TestMethod]
        public void OnePropertyOneScan()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah..Two");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[1]).Name);
        }

        [TestMethod]
        public void SinglePropertyAndIndexer()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[0]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
        }

        [TestMethod]
        public void SinglePropertyAndExistsQuery()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @..name ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Exists, expressions.Operator);
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name);
            Assert.AreEqual(1, paths.Count);
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @.name=='hi' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("hi", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithEscapeQuote()
        {
            JsonDocumentPath path = new JsonDocumentPath(@"Blah[ ?( @.name=='h\'i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h'i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithDoubleEscape()
        {
            JsonDocumentPath path = new JsonDocumentPath(@"Blah[ ?( @.name=='h\\i' ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual("h\\i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithRegexAndOptions()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @.name=~/hi/i ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/hi/i", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithRegex()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[?(@.title =~ /^.*Sword.*$/)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual("/^.*Sword.*$/", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithEscapedRegex()
        {
            JsonDocumentPath path = new JsonDocumentPath(@"Blah[?(@.title =~ /[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g)]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.RegexEquals, expressions.Operator);
            Assert.AreEqual(@"/[\-\[\]\/\{\}\(\)\*\+\?\.\\\^\$\|]/g", ((JsonElement)expressions.Right).GetString());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithOpenRegex()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath(@"Blah[?(@.title =~ /[\"); }, "Path ended with an open regex.");
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithUnknownEscape()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath(@"Blah[ ?( @.name=='h\i' ) ]"); }, @"Unknown escape character: \i");
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithFalse()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @.name==false ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(false, ((JsonElement)expressions.Right).GetBoolean());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithTrue()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @.name==true ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(true, ((JsonElement)expressions.Right).GetBoolean());
        }

        [TestMethod]
        public void SinglePropertyAndFilterWithNull()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[ ?( @.name==null ) ]");
            Assert.AreEqual(2, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[1]).Expression;
            Assert.AreEqual(QueryOperator.Equals, expressions.Operator);
            Assert.AreEqual(null, ((JsonElement)expressions.Right).GetObjectValue());
        }

        [TestMethod]
        public void FilterWithScan()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@..name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual("name", ((ScanFilter)paths[0]).Name);
        }

        [TestMethod]
        public void FilterWithNotEquals()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name<>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithNotEquals2()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name!=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.NotEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithLessThan()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name<null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThan, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithLessThanOrEquals()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name<=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.LessThanOrEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithGreaterThan()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThan, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithGreaterThanOrEquals()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>=null)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.GreaterThanOrEquals, expressions.Operator);
        }

        [TestMethod]
        public void FilterWithInteger()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>=12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12, ((JsonElement)expressions.Right).GetInt32());
        }

        [TestMethod]
        public void FilterWithNegativeInteger()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>=-12)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(-12, ((JsonElement)expressions.Right).GetInt32());
        }

        [TestMethod]
        public void FilterWithFloat()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(12.1d, ((JsonElement)expressions.Right).GetDouble());
        }

        [TestMethod]
        public void FilterExistWithAnd()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name&&@.title)]");
            CompositeExpression expressions = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, expressions.Operator);
            Assert.AreEqual(2, expressions.Expressions.Count);

            var first = (BooleanQueryExpression)expressions.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            var second = (BooleanQueryExpression)expressions.Expressions[1];
            var secondPaths = (List<PathFilter>)second.Left;
            Assert.AreEqual("title", ((FieldFilter)secondPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, second.Operator);
        }

        [TestMethod]
        public void FilterExistWithAndOr()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name&&@.title||@.pie)]");
            CompositeExpression andExpression = (CompositeExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(QueryOperator.And, andExpression.Operator);
            Assert.AreEqual(2, andExpression.Expressions.Count);

            var first = (BooleanQueryExpression)andExpression.Expressions[0];
            var firstPaths = (List<PathFilter>)first.Left;
            Assert.AreEqual("name", ((FieldFilter)firstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, first.Operator);

            CompositeExpression orExpression = (CompositeExpression)andExpression.Expressions[1];
            Assert.AreEqual(2, orExpression.Expressions.Count);

            var orFirst = (BooleanQueryExpression)orExpression.Expressions[0];
            var orFirstPaths = (List<PathFilter>)orFirst.Left;
            Assert.AreEqual("title", ((FieldFilter)orFirstPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orFirst.Operator);

            var orSecond = (BooleanQueryExpression)orExpression.Expressions[1];
            var orSecondPaths = (List<PathFilter>)orSecond.Left;
            Assert.AreEqual("pie", ((FieldFilter)orSecondPaths[0]).Name);
            Assert.AreEqual(QueryOperator.Exists, orSecond.Operator);
        }

        [TestMethod]
        public void FilterWithRoot()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?($.name>=12.1)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            List<PathFilter> paths = (List<PathFilter>)expressions.Left;
            Assert.AreEqual(2, paths.Count);
            Assert.IsInstanceOfType<RootFilter>(paths[0]);
            Assert.IsInstanceOfType<FieldFilter>(paths[1]);
        }

        [TestMethod]
        public void BadOr1()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||)]"), "Unexpected character while parsing path query: )");
        }

        [TestMethod]
        public void BaddOr2()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name|)]"), "Unexpected character while parsing path query: |");
        }

        [TestMethod]
        public void BaddOr3()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name|"), "Unexpected character while parsing path query: |");
        }

        [TestMethod]
        public void BaddOr4()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||"), "Path ended with open query.");
        }

        [TestMethod]
        public void NoAtAfterOr()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||s"), "Unexpected character while parsing path query: s");
        }

        [TestMethod]
        public void NoPathAfterAt()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||@"), @"Path ended with open query.");
        }

        [TestMethod]
        public void NoPathAfterDot()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||@."), @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void NoPathAfterDot2()
        {
            ExceptionAssert.Throws<JsonException>(() => new JsonDocumentPath("[?(@.name||@.)]"), @"Unexpected end while parsing path.");
        }

        [TestMethod]
        public void FilterWithFloatExp()
        {
            JsonDocumentPath path = new JsonDocumentPath("[?(@.name>=5.56789e+0)]");
            BooleanQueryExpression expressions = (BooleanQueryExpression)((QueryFilter)path.Filters[0]).Expression;
            Assert.AreEqual(5.56789e+0, ((JsonElement)expressions.Right).GetDouble());
        }

        [TestMethod]
        public void MultiplePropertiesAndIndexers()
        {
            JsonDocumentPath path = new JsonDocumentPath("Blah[0]..Two.Three[1].Four");
            Assert.AreEqual(6, path.Filters.Count);
            Assert.AreEqual("Blah", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual("Two", ((ScanFilter)path.Filters[2]).Name);
            Assert.AreEqual("Three", ((FieldFilter)path.Filters[3]).Name);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[4]).Index);
            Assert.AreEqual("Four", ((FieldFilter)path.Filters[5]).Name);
        }

        [TestMethod]
        public void BadCharactersInIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("Blah[[0]].Two.Three[1].Four"); }, @"Unexpected character while parsing path indexer: [");
        }

        [TestMethod]
        public void UnclosedIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("Blah[0"); }, @"Path ended with open indexer.");
        }

        [TestMethod]
        public void IndexerOnly()
        {
            JsonDocumentPath path = new JsonDocumentPath("[111119990]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void IndexerOnlyWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("[  10  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(10, ((ArrayIndexFilter)path.Filters[0]).Index);
        }

        [TestMethod]
        public void MultipleIndexes()
        {
            JsonDocumentPath path = new JsonDocumentPath("[111119990,3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [TestMethod]
        public void MultipleIndexesWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("[   111119990  ,   3   ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes.Count);
            Assert.AreEqual(111119990, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[0]);
            Assert.AreEqual(3, ((ArrayMultipleIndexFilter)path.Filters[0]).Indexes[1]);
        }

        [TestMethod]
        public void MultipleQuotedIndexes()
        {
            JsonDocumentPath path = new JsonDocumentPath("['111119990','3']");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [TestMethod]
        public void MultipleQuotedIndexesWithWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("[ '111119990' , '3' ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(2, ((FieldMultipleFilter)path.Filters[0]).Names.Count);
            Assert.AreEqual("111119990", ((FieldMultipleFilter)path.Filters[0]).Names[0]);
            Assert.AreEqual("3", ((FieldMultipleFilter)path.Filters[0]).Names[1]);
        }

        [TestMethod]
        public void SlicingIndexAll()
        {
            JsonDocumentPath path = new JsonDocumentPath("[111119990:3:2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndex()
        {
            JsonDocumentPath path = new JsonDocumentPath("[111119990:3]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexNegative()
        {
            JsonDocumentPath path = new JsonDocumentPath("[-111119990:-3:-2]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexEmptyStop()
        {
            JsonDocumentPath path = new JsonDocumentPath("[  -3  :  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexEmptyStart()
        {
            JsonDocumentPath path = new JsonDocumentPath("[ : 1 : ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(1, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(null, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void SlicingIndexWhitespace()
        {
            JsonDocumentPath path = new JsonDocumentPath("[  -111119990  :  -3  :  -2  ]");
            Assert.AreEqual(1, path.Filters.Count);
            Assert.AreEqual(-111119990, ((ArraySliceFilter)path.Filters[0]).Start);
            Assert.AreEqual(-3, ((ArraySliceFilter)path.Filters[0]).End);
            Assert.AreEqual(-2, ((ArraySliceFilter)path.Filters[0]).Step);
        }

        [TestMethod]
        public void EmptyIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("[]"); }, "Array index expected.");
        }

        [TestMethod]
        public void IndexerCloseInProperty()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("]"); }, "Unexpected character while parsing path: ]");
        }

        [TestMethod]
        public void AdjacentIndexers()
        {
            JsonDocumentPath path = new JsonDocumentPath("[1][0][0][" + int.MaxValue + "]");
            Assert.AreEqual(4, path.Filters.Count);
            Assert.AreEqual(1, ((ArrayIndexFilter)path.Filters[0]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[1]).Index);
            Assert.AreEqual(0, ((ArrayIndexFilter)path.Filters[2]).Index);
            Assert.AreEqual(int.MaxValue, ((ArrayIndexFilter)path.Filters[3]).Index);
        }

        [TestMethod]
        public void MissingDotAfterIndexer()
        {
            ExceptionAssert.Throws<JsonException>(() => { new JsonDocumentPath("[1]Blah"); }, "Unexpected character following indexer: B");
        }

        [TestMethod]
        public void PropertyFollowingEscapedPropertyName()
        {
            JsonDocumentPath path = new JsonDocumentPath("frameworks.dnxcore50.dependencies.['System.Xml.ReaderWriter'].source");
            Assert.AreEqual(5, path.Filters.Count);

            Assert.AreEqual("frameworks", ((FieldFilter)path.Filters[0]).Name);
            Assert.AreEqual("dnxcore50", ((FieldFilter)path.Filters[1]).Name);
            Assert.AreEqual("dependencies", ((FieldFilter)path.Filters[2]).Name);
            Assert.AreEqual("System.Xml.ReaderWriter", ((FieldFilter)path.Filters[3]).Name);
            Assert.AreEqual("source", ((FieldFilter)path.Filters[4]).Name);
        }
    }
}