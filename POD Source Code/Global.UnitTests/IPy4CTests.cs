﻿//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NUnit.Framework;
using System.Collections.Generic;
using POD;

namespace Global.UnitTests
{
    [TestFixture]
    public class IPy4CTests
    {
        /// <summary>
        /// Tests for GetMaxPrecision() function
        /// </summary>    
        [Test]
        public void GetMaxPrecision_ListIsEmpty_ReturnsPrecisionOf0()
        {
            //Arrange
            List<double> podItems = new List<double>();
            //Act
            int result = IPy4C.GetMaxPrecision(podItems);

            //Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        [TestCase(1.0)]
        [TestCase(-1.0)]
        [TestCase(0.0)]
        public void GetMaxPrecision_ListHoldsAnInteger_ReturnsPrecision0(double testItem)
        {
            //Arrange
            List<double> podItems = new List<double>() { testItem };
            //Act
            int result = IPy4C.GetMaxPrecision(podItems);

            //Assert
            Assert.That(result, Is.EqualTo(0));
        }
        [Test]
        [TestCase(1.1)]
        [TestCase(.1)]
        [TestCase(-.1)]
        [TestCase(-1.1)]
        public void GetMaxPrecision_ListContainsADecimal_ReturnsPrecisionOf1(double testItem)
        {
            //Arrange
            List<double> podItems = new List<double>() { testItem };
            //Act
            int result = IPy4C.GetMaxPrecision(podItems);

            //Assert
            Assert.That(result, Is.EqualTo(1));
        }
        [Test]
        [TestCase(new double[] { .1, .12, .123 })]
        [TestCase(new double[] { .123, .12, .1 })]
        [TestCase(new double[] { .1, .123, .12 })]
        [TestCase(new double[] { -.1, .12, .123 })]
        [TestCase(new double[] { .123, -.12, .1 })]
        [TestCase(new double[] { .1, .123, -.12 })]
        [TestCase(new double[] { 1.1, 1.12, 1.123 })]
        [TestCase(new double[] { 1.1, -1.12, -1.123 })]
        public void GetMaxPrecision_ListContainsVaryingLenthDecimals_ReturnsAPrecisionOf3(double[] myArray)
        {
            //Arrange
            List<double> podItems = new List<double>();
            podItems.Add(myArray[0]);
            podItems.Add(myArray[1]);
            podItems.Add(myArray[2]);
            //Act
            int result = IPy4C.GetMaxPrecision(podItems);

            //Assert
            Assert.That(result, Is.EqualTo(3));
        }
        /// <summary>
        /// Unit tests for GetMaxPrecisionDict. The responses are a dictionary type
        /// in signal response, so this meethod is used to deal with multiple responses
        /// </summary>
        [Test]
        public void GetMaxPrecisionDict_OneDictionaryItem_Returns3()
        {
            //Arrange
            Dictionary<string, List<double>> podDict = new Dictionary<string, List<double>>();
            podDict.Add("test1,", new List<double> { .123, .12, .1 });

            //Act
            int result = IPy4C.GetMaxPrecisionDict(podDict);

            //Arrange
            Assert.That(result, Is.EqualTo(3));

        }
        [Test]
        public void GetMaxPrecisionDict_TwoDictionaryItems_Returns3()
        {
            //Arrange
            Dictionary<string, List<double>> podDict = new Dictionary<string, List<double>>();
            podDict.Add("test1,", new List<double> { .123, .12, .1 });
            podDict.Add("test2,", new List<double> { .1, .2, .3 });
            //Act
            int result = IPy4C.GetMaxPrecisionDict(podDict);

            //Arrange
            Assert.That(result, Is.EqualTo(3));

        }
    }
}
