﻿// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Templates.Analyzer.RuleEngines.JsonEngine;
using Microsoft.Azure.Templates.Analyzer.RuleEngines.JsonEngine.Expressions;
using Microsoft.Azure.Templates.Analyzer.RuleEngines.JsonEngine.Operators;
using Microsoft.Azure.Templates.Analyzer.Types;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Microsoft.Azure.Templates.Analyzer.JsonRuleEngine.UnitTests
{
    [TestClass]
    public class AllOfExpressionTests
    {
        [DataTestMethod]
        [DataRow(true, true, DisplayName = "AllOf evaluates to true (true && true)")]
        [DataRow(true, false, DisplayName = "AllOf evaluates to false (true && false)")]
        [DataRow(false, false, DisplayName = "AllOf evaluates to false (false && false)")]
        [DataRow(false, false, "ResourceProvider/resource", DisplayName = "AllOf - scoped to resourceType - evaluates to false (false && false)")]
        public void Evaluate_TwoLeafExpressions_ExpectedResultIsReturned(bool evaluation1, bool evaluation2, string resourceType = null)
        {
            // Arrange
            var mockJsonPathResolver = new Mock<IJsonPathResolver>();

            // This AllOf will have 2 expressions
            var mockOperator1 = new Mock<LeafExpressionOperator>().Object;
            var mockOperator2 = new Mock<LeafExpressionOperator>().Object;

            var mockLeafExpression1 = new Mock<LeafExpression>(new object[] { "ResourceProvider/resource", "some.path", mockOperator1 });
            var mockLeafExpression2 = new Mock<LeafExpression>(new object[] { "ResourceProvider/resource", "some.path", mockOperator2 });

            var jsonRuleResult1 = new JsonRuleResult
            {
                Passed = evaluation1
            };

            var jsonRuleResult2 = new JsonRuleResult
            {
                Passed = evaluation2
            };

            mockJsonPathResolver
                .Setup(s => s.Resolve(It.Is<string>(path => path == "some.path")))
                .Returns(new List<IJsonPathResolver> { mockJsonPathResolver.Object });

            if (!string.IsNullOrEmpty(resourceType))
            {
                mockJsonPathResolver
                    .Setup(s => s.ResolveResourceType(It.Is<string>(type => type == resourceType)))
                    .Returns(new List<IJsonPathResolver> { mockJsonPathResolver.Object });
            }

            var results1 = new JsonRuleResult[] { jsonRuleResult1 };
            var results2 = new JsonRuleResult[] { jsonRuleResult2 };

            mockLeafExpression2
                .Setup(s => s.Evaluate(mockJsonPathResolver.Object))
                .Returns(new Evaluation(evaluation1, results1));

            mockLeafExpression1
                .Setup(s => s.Evaluate(mockJsonPathResolver.Object))
                .Returns(new Evaluation(evaluation2, results2));

            var expressionArray = new Expression[] { mockLeafExpression2.Object, mockLeafExpression1.Object };

            var allOfExpression = new AllOfExpression(expressionArray, resourceType);

            // Act
            var allOfEvaluation = allOfExpression.Evaluate(mockJsonPathResolver.Object);

            // Assert
            bool expectedAllOfEvaluation = evaluation1 && evaluation2;
            Assert.AreEqual(expectedAllOfEvaluation, allOfEvaluation.Passed);
            Assert.AreEqual(2, allOfEvaluation.Results.Count());
            int expectedTrue = 0;
            int expectedFalse = 0;

            if (evaluation1)
                expectedTrue++;
            else
                expectedFalse++;

            if (evaluation2)
                expectedTrue++;
            else
                expectedFalse++;

            Assert.AreEqual(expectedTrue, allOfEvaluation.GetResultsEvaluatedTrue().Count());
            Assert.AreEqual(expectedFalse, allOfEvaluation.GetResultsEvaluatedFalse().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Evaluate_NullScope_ThrowsException()
        {
            var allOfExpression = new AllOfExpression(new Expression[] { });
            allOfExpression.Evaluate(jsonScope: null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NullExpressions_ThrowsException()
        {
            new AllOfExpression(null);
        }
    }
}
