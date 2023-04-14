﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using POD;
using CSharpBackendWithR;
using System.Windows.Forms;
using Moq;
using System.Text.RegularExpressions;
using System.Drawing;

namespace Global.UnitTests
{
    [TestFixture]
    public class GlobalsTests
    {
        //private Mock<IContainerControl> _mockControl;
        private Control _control;
        private string latinString;
        private string latinStringWLineBreak;
        [SetUp]
        public void Setup()
        {
            _control = new Control();

            latinString= "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod"+
                         "tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam,"+
                          "quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo"+
                          "consequat.Duis aute irure dolor in reprehenderit in voluptate velit esse"+
                          "cillum dolore eu fugiat nulla pariatur.Excepteur sint occaecat cupidatat non"+
                          "proident, sunt in culpa qui";
            latinStringWLineBreak = latinString + '\n';
        }
        /// <summary>
        /// Tests for GetLabelIntervalBasedOnChartSize(Control chart, AxisKind kind) function
        /// </summary>
        /// <param name="axisKind"></param>
        [Test]
        [TestCase (AxisKind.X)]
        [TestCase(AxisKind.Y)]
        public void GetLabelIntervalBasedOnChartSize_ChartIsNull_ReturnsDefaultLabelCount(AxisKind axisKind)
        {
            var result=Globals.GetLabelIntervalBasedOnChartSize(null, axisKind);

            Assert.That(result, Is.EqualTo(Globals.DefaultLabelCount));
        }
        [Test]
        [TestCase(AxisKind.X)]
        [TestCase(AxisKind.Y)]
        public void GetLabelIntervalBasedOnChartSize_ChartIsNullAndAxisKindIsX_ReturnsInt32(AxisKind axisKind)
        {
            _control.Width = 500;
            _control.Height = 500;
            var result = Globals.GetLabelIntervalBasedOnChartSize(_control, axisKind);
            
            Assert.That(result, Is.GreaterThanOrEqualTo(1));
            Assert.That(result, Is.Not.EqualTo(10));
        }
        /// <summary>
        /// tests for StdWidth(Control control) function
        /// </summary>
        [Test]
        public void StdWidth_ControlIsNull_Returns70()
        {
            //Act
            var result = Globals.StdWidth(null);

            Assert.That(result, Is.EqualTo(70));

        }
        [Test]
        public void StdWidth_ControlIsNotNull_Returns70TimesDpiXOver96()
        {
            var testScale = _control.CreateGraphics();
            var scale = testScale.DpiX / 96.0;
            testScale.Dispose();
            //Act
            var result = Globals.StdWidth(_control);
            //Assert
            Assert.That(result, Is.EqualTo(70* scale));
        }
        /// <summary>
        /// tests for StdHeight(Control control) function
        /// </summary>
        [Test]
        public void Stdheight_ControlIsNotNull_ReturnsAnInt32()
        {
            //act
            var result = Globals.StdHeight(_control);
            //Assert
            Assert.That(result, Is.TypeOf<Int32>());
        }
        /// <summary>
        /// tests for CleanColumn(string myName) function
        /// </summary>
        [Test]
        [TestCase("ThisStringContainsNoInvalidCharacters")]
        [TestCase("This String Contains No Invalid Characters")]
        public void CleanColumnName_StringContainsNoInvalidCharacters_ReturnsTheSameColumnName(string testString)
        {
            var result = Globals.CleanColumnName(testString);

            Assert.That(result, Is.EqualTo(testString));
        }

        [Test]
        [TestCase("<This!String@Contains#No$Invalid%Characters>")]
        public void CleanColumnName_StringHasInvalidCharactersButDoesntTimeOut_ReturnsTheColumnNameWithoutIllegalsChars(string testString)
        {
            var result = Globals.CleanColumnName(testString);

            Assert.That(result, Is.EqualTo("ThisStringContainsNoInvalidCharacters"));
        }
        /*
        [Test]
        public void CleanColumnName_StringHasInvalidCharactersBuTimesOut_ReturnsAnEmptyString()
        {
            //Arrange
            string largeString = GenerateLargeRandomString(50000000);
            //Act
            var result = Globals.CleanColumnName(largeString);
            //Assert
            Assert.That(result, Is.EqualTo(String.Empty));
            Assert.Throws<RegexMatchTimeoutException>(() => Globals.CleanColumnName(largeString));

        }
        private string GenerateLargeRandomString(int stringLength)
        {
            Random rd = new Random();
            //const string allowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";
            const string allowedChars = @"!@$~()#\\/=><+/*%&|^'""[\]";
            char[] chars = new char[stringLength];

            for (int i = 0; i < stringLength; i++)
            {
                chars[i] = allowedChars[rd.Next(0, allowedChars.Length)];
            }


            return new string(chars);
        }
        */
        ///<summary>
        /// tests for CreateCursorNoResize(Bitmap bmp, int xHotSpot, int yHotSpot)
        ///</summary>
        [Test]
        public void CreateCursorNoResize_ValidBitmapPassed_ReturnsACustomCursor()
        {
            //Arrange
            Cursor resultCursor = null;
            //Act
            using(Bitmap bmp=new Bitmap(10,10))
            {
                resultCursor = Globals.CreateCursorNoResize(bmp, 1, 1);
            }
            Assert.That(resultCursor, Is.Not.EqualTo(Cursors.Default));
        }
        [Test]
        [Ignore("Figure out how to inject message box for this test")]
        public void CreateCursorNoResize_NullBitmapPassed_ReturnsDefaultCursor()
        {
            //Arrange
            Cursor resultCursor = null;
            
            //Act
            resultCursor = Globals.CreateCursorNoResize(null, 1, 1);;
            //Assert
            Assert.That(resultCursor, Is.EqualTo(Cursors.Default));
        }
        ///<summary>
        /// DeleteMRUList(string value, string fileName) is too difficult to unit test
        ///</summary>

        ///<summary>
        /// UpdateMRUListMultiLine(string value, string fileName, int maxLines = 8) is too difficult to unit test
        ///</summary>

        ///<summary>
        ///  UpdateMRUList(string value, string fileName, bool useSplitted = false, string splitCharacter = "|", int maxLines = 8) is too difficult to unit test
        ///</summary>

        ///<summary>
        /// GetMRUListMultiLine(string fileName) is too difficult to unit test
        ///</summary>

        ///<summary>
        /// GetMRUList(string fileName) is too difficult to unit test
        ///</summary>

        ///<summary>
        /// GetMRUListWithoutEmpties(string fileName, bool useSplitted = false, string splitCharacter = "|") is too difficult to unit test
        ///</summary>

        ///<summary>
        /// CleanUpRandomImageFiles() is too difficult to unit test
        ///</summary>

        ///<summary>
        /// Tests for SplitIntoLines(string p) function
        ///</summary>
        [Test]
        public void SplitIntoLines_StringWithNoSpaces_ReturnsTheSameString()
        {
            var myString = "ThisIsAStringWithNoSpaces";

            string result = Globals.SplitIntoLines(myString);

            Assert.That(result, Is.EqualTo(myString));
        }
        [Test]
        public void SplitIntoLines_EmptyString_ReturnsEmptyString()
        {
            var myString = "";

            string result = Globals.SplitIntoLines(myString);

            Assert.That(result, Is.EqualTo(String.Empty));
        }
        [Test]
        public void SplitIntoLines_StringHasWordsButNotLongerThan40_ReturnsTheStringWithLineBreaksWithoutEndingWithALineBreak()
        {
            //arrange
            var myString = latinString;
            //Act
            string result = Globals.SplitIntoLines(myString);


            Assert.That(result, Is.Not.EqualTo(myString));
            Assert.That(result.Contains('\n'));
            Assert.That(result.EndsWith("\n")== false);
            Assert.That(result.EndsWith(" ") == false);
        }
        [Test]
        public void SplitIntoLines_StringHasWordsButNotLongerThan40AndEndsInLineBreak_ReturnsTheStringWithLineBreaksWithoutEndingWithALineBreak()
        {
            //arrange
            var myString = latinStringWLineBreak;
            //Act
            string result = Globals.SplitIntoLines(myString);

            //Assert
            Assert.That(result, Is.Not.EqualTo(myString));
            Assert.That(result.Contains('\n'));
            Assert.That(result.EndsWith("\n") == false);
            Assert.That(result.EndsWith(" ") == false);
        }



    }
}
