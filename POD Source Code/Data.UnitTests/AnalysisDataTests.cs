﻿using NUnit.Framework;
using System;
using Moq;
using POD.Data;
using System.Collections.Generic;
using System.Data;
using POD;
using CSharpBackendWithR;
using POD.ExcelData;
using static POD.Data.SortPoint;
using System.Linq;

namespace Data.UnitTests
{
    [TestFixture]
    public class AnalysisDataTests
    {
        private AnalysisData _data;
        private DataSource _source;
        private DataTable _table;
        Mock<IExcelWriterControl> _excelWriterControl;
        Mock<IExcelExport> _excelExport;
        private Mock<I_IPy4C> _python;
        private Mock<IUpdateTableControl> _updateTables;
        private Mock<IFlipBinarySign> _flipBinaryControl;
        [SetUp]
        public void Setup()
        {
            _updateTables = new Mock<IUpdateTableControl>();
            _flipBinaryControl = new Mock<IFlipBinarySign>();
            _data = new AnalysisData(_updateTables.Object, _flipBinaryControl.Object);
            _source = new DataSource("MyDataSource", "ID", "flawName.centimeters", "Response");
            _table = new DataTable();
            _excelWriterControl = new Mock<IExcelWriterControl>();
            _excelExport = new Mock<IExcelExport>();
            GenerateSampleTable();

        }
        private void GenerateSampleTable()
        {
            _table.Columns.Add("Column1");
            _table.Columns.Add("Column2");
            _table.Columns.Add("Column3");
        }
        /// <summary>
        /// tests ActivatedFlawName Getter
        /// </summary>
        [Test]
        public void ActivatedFlawName_ActivatedFlawCountIsZero_ReturnsEmptyString()
        {
            //Arrange
            //Act
            var result = _data.ActivatedFlawName;
            //Assert
            Assert.AreEqual(result, string.Empty);
        }
        [Test]
        public void ActivatedFlawName_ActivatedFlawCountGreaterThanZero_ReturnsActivatedFlawName()
        {
            //Arrange
            _data.SetSource(_source);
            //Act
            var result = _data.ActivatedFlawName;
            //Assert
            Assert.AreEqual(result, "flawName.centimeters");
        }
        /// <summary>
        /// tests ActivatedOriginalFlawName Getter
        /// </summary>
        [Test]
        public void ActivatedOriginalFlawName_NamesCountIsZero_ReturnsEmptyString()
        {
            //Arrange
            //Act
            var result = _data.ActivatedOriginalFlawName;
            //Assert
            Assert.AreEqual(result, string.Empty);
        }
        [Test]
        public void ActivatedOriginalFlawName_NamesCountGreaterThanZero_ReturnsActivatedFlawName()
        {
            //Arrange
            _data.SetSource(_source);
            _data.ActivateFlaw("flawName.centimeters");
            //_data.ActivateFlaws(new List<string>());
            //Act
            var result = _data.ActivatedOriginalFlawName;
            //Assert
            Assert.AreEqual(result, "flawName.centimeters");
        }
        /// Tests for GetRemovedPointComment(int myColIndex, int myRowIndex) function
        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void GetRemovedPointComment_ColumnIndexIsNotThere_AddsToTheDictionaryAndReturnsEmptyString(int columnIndex)
        {
            //Arrange
            var _ = _data.CommentDictionary;
            var originalCount = _data.CommentDictionary.Count;
            //Act 
            var result = _data.GetRemovedPointComment(columnIndex, 1);
            //Assert
            Assert.AreEqual(result, string.Empty);
            Assert.IsTrue(_data.CommentDictionary.ContainsKey(columnIndex));
            Assert.AreEqual(_data.CommentDictionary.Count, originalCount + 1);
        }
        /// Tests for GetRemovedPointComment(int myColIndex, int myRowIndex) function
        [Test]
        [TestCase(1, 4)]
        [TestCase(2, 5)]
        [TestCase(3, 6)]
        public void GetRemovedPointComment_ColumnIndexIsThereButRowIsNot_AddsToTheDictionaryAndReturnsEmptyString(int columnIndex, int rowIndex)
        {
            //Arrange
            var _ = _data.CommentDictionary;
            //Alter the row index so that the dictionary contains the column, but not the row
            _data.CommentDictionary.Add(columnIndex, new Dictionary<int, string>() { { rowIndex + 1, "" } });
            var originalCount = _data.CommentDictionary.Count;
            var originalColIndexCount = _data.CommentDictionary[columnIndex].Count;
            //Act 
            var result = _data.GetRemovedPointComment(columnIndex, rowIndex);
            //Assert
            Assert.AreEqual(result, string.Empty);
            Assert.IsTrue(_data.CommentDictionary.ContainsKey(columnIndex));
            Assert.IsTrue(_data.CommentDictionary[columnIndex].ContainsKey(rowIndex));
            // Dictionary size stays the same, but the col index dictionary gets bigger
            Assert.AreEqual(_data.CommentDictionary.Count, originalCount);
            Assert.AreEqual(_data.CommentDictionary[columnIndex].Count, originalColIndexCount + 1);
        }
        [Test]
        [TestCase(1, 4)]
        [TestCase(2, 5)]
        [TestCase(3, 6)]
        public void GetRemovedPointComment_ColumnAndRowIndexAreBothThere_ReturnsTheRemovedPointComment(int columnIndex, int rowIndex)
        {
            //Arrange
            var _ = _data.CommentDictionary;
            //Alter the row index so that the dictionary contains the column, but not the row
            _data.CommentDictionary.Add(columnIndex, new Dictionary<int, string>() { { rowIndex, "MyRemovedPointComment" } });
            var originalCount = _data.CommentDictionary.Count;
            var originalColIndexCount = _data.CommentDictionary[columnIndex].Count;
            //Act 
            var result = _data.GetRemovedPointComment(columnIndex, rowIndex);
            Assert.AreEqual(_data.CommentDictionary.Count, originalCount);
            Assert.AreEqual(_data.CommentDictionary[columnIndex].Count, originalColIndexCount);
        }
        /// Tests for SetRemovedPointComment(int myColIndex, int myRowIndex, string myComment) function
        [Test]
        public void SetRemovedPointComment_ValidCommentPassed_AssignsTheValueInTheDictionary()
        {
            //Arrange
            var _ = _data.CommentDictionary;
            _data.CommentDictionary.Add(1, new Dictionary<int, string>() { { 1, "MyRemovedPointComment" } });
            //Act
            _data.SetRemovedPointComment(1, 1, "MyNewRemovePointComment");
            //Assert
            Assert.AreEqual(_data.CommentDictionary[1][1], "MyNewRemovePointComment");
        }
        /*
         * TODO: figure out why the duplicate table tests are being passed by reference
         * It's possible this function may not even be necessary
        /// Tests For DuplicateTable(DataTable fromTable, DataTable toTable)
        [Test]
        public void DuplicateTable_FromTableIsNull_ToTableBecomesNull()
        {
            //Arrange
            DataTable fromTable = null;
            DataTable toTable = new DataTable();
            //Act
            AnalysisData.DuplicateTable(fromTable, toTable);
            //Assert
            Assert.IsNull(toTable);
        }
        [Test]
        public void DuplicateTable_FromTableIsNotNullAndToTableIsNull_TableDuplicated()
        {
            //Arrange
            DataTable fromTable = new DataTable();
            DataTable toTable = null;
            //Act
            AnalysisData.DuplicateTable(fromTable, toTable);
            //Assert
            Assert.IsNotNull(toTable);
        }
        [Test]
        public void DuplicateTable_fromTableAndToTableBothNotNull_TableDuplicated()
        {
            //Arrange
            DataTable fromTable = _table;
            DataTable toTable = new DataTable();
            //Act
            AnalysisData.DuplicateTable(fromTable, toTable);
            //Assert
            Assert.AreEqual(fromTable, toTable);
            Assert.IsFalse(ReferenceEquals(fromTable, toTable));
        }
        */
        ///Tests for the SetSource(DataSource mySource, List<string> myFlaws, List<string> myMetaDatas,
        ///List<string> myResponses, List<string> mySpecIDs)
        /// function
        [Test]
        public void SetSource_OriginalRowsCountIsZero_ColumnsAddedToQuickTable()
        {
            //Arrange
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            //Act
            _data.SetSource(_source, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            //Assert
            Assert.AreEqual(_data.QuickTable.Columns.Count, 3);
        }
        [Test]
        public void SetSource_OriginalRowsCountIsNotZeroZero_ColumnsNotToQuickTable()
        {
            //Arrange
            _source.Original.Rows.Add(new List<string> { "1" });
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            //Act
            _data.SetSource(_source, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            //Assert
            Assert.That(_data.QuickTable.Columns.Count, Is.Zero);
        }
        [Test]
        public void SetSource_DataTypeIsNothitMiss_FlawTransformSetToLinear()
        {
            //Arrange
            var ahatTable = CreateSampleDataTable();
            for (int i = 0; i < 10; i++)
                ahatTable.Rows.Add(i, i * .25, i + .1);
            DataSource sourceWithActualData = SetupSampleDataSource(ahatTable);
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            //Act
            _data.SetSource(sourceWithActualData, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            //Assert
            Assert.That(_data.FlawTransform, Is.EqualTo(TransformTypeEnum.Linear));
        }
        [Test]
        public void SetSource_DataTypeIshitMiss_FlawTransformSetToLog()
        {
            //Arrange
            var hitmissTable = CreateSampleDataTable();
            for (int i = 0; i < 10; i++)
                hitmissTable.Rows.Add(i, i * .25, i % 2);
            DataSource sourceWithActualData = SetupSampleDataSource(hitmissTable);
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            //Act
            _data.SetSource(sourceWithActualData, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            //Assert
            Assert.That(_data.FlawTransform, Is.EqualTo(TransformTypeEnum.Log));
        }
        private DataTable CreateSampleDataTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("ID");
            table.Columns.Add("flawName.centimeters");
            table.Columns.Add("Response");
            return table;
        }
        private void SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas,
            out List<string> myResponses, out List<string> mySpecIDs)
        {
            myFlaws = new List<string> { "flawName.centimeters" };
            myMetaDatas = new List<string>();
            myResponses = new List<string> { "Response" };
            mySpecIDs = new List<string> { "ID" };
        }
        private DataSource SetupSampleDataSource(DataTable table)
        {
            return new DataSource(table,
                new TableRange(RangeNames.SpecID) { Range = new List<int> { 0 }, Count = 1, StartIndex = 0, MaxIndex = 0 },
                new TableRange(RangeNames.MetaData),
                new TableRange(RangeNames.FlawSize) { Range = new List<int> { 1 }, Count = 1, StartIndex = 1, MaxIndex = 1 },
                 new TableRange(RangeNames.Response) { Range = new List<int> { 2 }, Count = 1, StartIndex = 2, MaxIndex = 2 });
        }
        /*
        /// <summary>
        /// Tests for the TurnAllPointsOn() function
        /// </summary>
        [Test]
        public void TurnAllPointsOn_TurnedOffPointsExistInTheData_AllPointsAreTurnedOnAndListBecomesEmpty()
        {
            //Arrange
            _data.TurnedOffPoints.Add(new DataPointIndex(1,1,"first"));
            _data.TurnedOffPoints.Add(new DataPointIndex(1, 2, "second"));
            _data.TurnedOffPoints.Add(new DataPointIndex(1, 3, "third"));
            //Act
            _data.TurnAllPointsOn();
            //Assert
            Assert.That(_data.TurnedOffPoints.Count, Is.Zero);
        }
        */
        /// <summary>
        /// Tests for the TurnOffPoint() function
        /// </summary>
        /// can't really test this one either 

        /// Tests for UpdateData(bool quickFlag = false)
        [Test]
        public void UpdateData_PythonIsNull_DataNotUpdated()
        {
            //Arrange
            _data.SetPythonEngine(null, "Name");
            SetupHitMissAnalysisObject(new List<double> { .1, .2, .3 },
                new Dictionary<string, List<double>> { { "Responses", new List<double>() { 0.0, 0.0, 1.0 } } });
            SetupAHatAnalysisObject(new List<double> { .1, .2, .3 },
                new Dictionary<string, List<double>> { { "Responses", new List<double>() { 1.0, 2.0, 3.0 } } });
            //Act
            _data.UpdateData();
            //Assert
            AssertFlawResponseDataNotUpdated();
        }
        [Test]
        public void UpdateData_PythonNotNullAndBothAnalysisObjectsAreNull_DataNotUpdated()
        {
            //Arrange
            _data.SetPythonEngine(new Mock<I_IPy4C>().Object, "Name");
            //Act
            Assert.DoesNotThrow(() => _data.UpdateData());
            _data.UpdateData();
            //Assert
            Assert.That(_data.HMAnalysisObject, Is.Null);
            Assert.That(_data.AHATAnalysisObject, Is.Null);
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void UpdateData_PythonNotNullAndAHatAnalysisNotNullAndDataTypeIsInvalid_OnlyDataColumnNamesUpdated(AnalysisDataTypeEnum dataType)
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 1.0, 2.0, 3.0 } } });
            _data.DataType = dataType;
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.Not.EqualTo("Response"));
            Assert.That(_data.AHATAnalysisObject.Flaws.Count, Is.Zero);
            Assert.That(_data.AHATAnalysisObject.Flaws_All.Count, Is.EqualTo(1));
            Assert.That(_data.AHATAnalysisObject.Responses.Count, Is.Zero);
            Assert.That(_data.AHATAnalysisObject.Responses_all.Count, Is.EqualTo(1));
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void UpdateData_PythonNotNullAndHitMissAnalysisNotNullAndDataTypeIsInvalid_OnlyDataColumnNamesUpdated(AnalysisDataTypeEnum dataType)
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 0.0, 0.0, 1.0 } } });
            _data.DataType = dataType;
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.Not.EqualTo("Response"));
            Assert.That(_data.HMAnalysisObject.Flaws.Count, Is.Zero);
            Assert.That(_data.HMAnalysisObject.Flaws_All.Count, Is.EqualTo(1));
            Assert.That(_data.HMAnalysisObject.Responses.Count, Is.Zero);
            Assert.That(_data.HMAnalysisObject.Responses_all.Count, Is.EqualTo(1));
        }
        [Test]
        public void UpdateData_AHatAnalysisNotNullButFlawsAllAndResponsesAllNotZeroAndQuickFlagFalse_OnlyDataColumnNamesUpdated()
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 1.0, 2.0, 3.0 } } });
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.EqualTo("Response"));
            Assert.That(_data.AHATAnalysisObject.Flaws.Count, Is.GreaterThan(1));
            Assert.That(_data.AHATAnalysisObject.Flaws_All.Count, Is.EqualTo(1));
            Assert.AreNotEqual(_data.AHATAnalysisObject.Responses, _data.AHATAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_AHatAnalysisNotNullButFlawsAllAndResponsesAllNotZeroAndQuickFlagTrue_DataColumnNamesFlawsAndResponsesUpdated()
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 1.0, 2.0, 3.0 } } });
            //Act
            _data.UpdateData(true);
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.EqualTo("Response"));
            Assert.AreEqual(_data.AHATAnalysisObject.Flaws, _data.AHATAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.AHATAnalysisObject.Responses, _data.AHATAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_HMAnalysisNotNullButFlawsAllAndResponsesAllNotZeroAndQuickFlagFalse_OnlyDataColumnNamesUpdated()
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 0.0, 0.0, 1.0 } } });
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.EqualTo("Response"));
            Assert.That(_data.HMAnalysisObject.Flaws.Count, Is.GreaterThan(1));
            Assert.That(_data.HMAnalysisObject.Flaws_All.Count, Is.EqualTo(1));
            Assert.AreNotEqual(_data.HMAnalysisObject.Responses, _data.HMAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_HMAnalysisNotNullButFlawsAllAndResponsesAllNotZeroAndQuickFlagTrue_DataColumnNamesFlawsAndResponsesUpdated()
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>() { 1.0 },
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 0.0, 0.0, 1.0 } } });
            //Act
            _data.UpdateData(true);
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.EqualTo("Response"));
            Assert.AreEqual(_data.HMAnalysisObject.Flaws, _data.HMAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.HMAnalysisObject.Responses, _data.HMAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_AHatAnalysisNotNullAndFlawsAllAreZeroAndResponsesAreNOTZero_DataColumnNamesAndFlawsUpdated()
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>(),
                new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() { 1.0, 2.0, 3.0 } } });
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.EqualTo("Response"));
            Assert.AreEqual(_data.AHATAnalysisObject.Flaws, _data.AHATAnalysisObject.Flaws_All);
            Assert.AreNotEqual(_data.AHATAnalysisObject.Responses, _data.AHATAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_AHatAnalysisNotNullAndFlawsAllAreNotZeroAndResponsesAreZero_DataColumnNamesAndResponsesUpdated()
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>() { -1.0 }, new Dictionary<string, List<double>>());
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.EqualTo("Response"));
            Assert.AreNotEqual(_data.AHATAnalysisObject.Flaws, _data.AHATAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.AHATAnalysisObject.Responses, _data.AHATAnalysisObject.Responses_all);
        }
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void UpdateData_AHatAnalysisNotNullAndFlawsAllAreZeroANDResponsesAreZero_DataColumnNamesAndResponsesUpdated(bool quickFlag)
        {
            //Arrange
            SetupActivationDataSignalResponse();
            SetupAHatAnalysisObject(new List<double>(), new Dictionary<string, List<double>>());
            //Act
            _data.UpdateData(quickFlag);
            //Assert
            Assert.That(_data.AHATAnalysisObject.SignalResponseName, Is.EqualTo("Response"));
            Assert.AreEqual(_data.AHATAnalysisObject.Flaws, _data.AHATAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.AHATAnalysisObject.Responses, _data.AHATAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_HitMissAnalysisNotNullAndFlawsAllZeroResponsesAreNOTZero_DataColumnNamesAndFlawsUpdated()
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>(), new Dictionary<string, List<double>>() { { "FakeResponse", new List<double>() } });
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.EqualTo("Response"));
            Assert.AreEqual(_data.HMAnalysisObject.Flaws, _data.HMAnalysisObject.Flaws_All);
            Assert.AreNotEqual(_data.HMAnalysisObject.Responses, _data.HMAnalysisObject.Responses_all);
        }
        [Test]
        public void UpdateData_HitMissAnalysisNotNullAndFlawsAllNOTZeroResponsesAreZero_DataColumnNamesAndResponsesUpdated()
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>() { -1.0 }, new Dictionary<string, List<double>>());
            //Act
            _data.UpdateData();
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.EqualTo("Response"));
            Assert.AreNotEqual(_data.HMAnalysisObject.Flaws, _data.HMAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.HMAnalysisObject.Responses, _data.HMAnalysisObject.Responses_all);
        }
        // Quick flag doesn't matter in this case
        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void UpdateData_HitMissAnalysisNotNullAndFlawsAllAreZeroAndResponsesAreZero_DataColumnNamesAndFlawsResponsesUpdated(bool quickFlag)
        {
            //Arrange
            SetupActivationDataHitMiss();
            SetupHitMissAnalysisObject(new List<double>(), new Dictionary<string, List<double>>());
            //Act
            _data.UpdateData(quickFlag);
            //Assert
            Assert.That(_data.HMAnalysisObject.HitMiss_name, Is.EqualTo("Response"));
            Assert.AreEqual(_data.HMAnalysisObject.Flaws, _data.HMAnalysisObject.Flaws_All);
            Assert.AreEqual(_data.HMAnalysisObject.Responses, _data.HMAnalysisObject.Responses_all);
        }
        private void SetupAHatAnalysisObject(List<double> flawsAll, Dictionary<string, List<double>> responsesAll)
        {
            _data.AHATAnalysisObject = new AHatAnalysisObject("AHat Analysis")
            {
                Flaws_All = flawsAll,
                Responses_all = responsesAll
            };
        }
        private void SetupHitMissAnalysisObject(List<double> flawsAll, Dictionary<string, List<double>> responsesAll)
        {
            _data.HMAnalysisObject = new HMAnalysisObject("HitMissAnalysis")
            {
                Flaws_All = flawsAll,
                Responses_all = responsesAll
            };
        }
        private void SetupActivationDataHitMiss()
        {
            var hitmissTable = CreateSampleDataTable();
            for (int i = 0; i < 10; i++)
                hitmissTable.Rows.Add(i, i * .25, i % 2);
            DataSource sourceWithActualData = SetupSampleDataSource(hitmissTable);
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            _data.SetSource(sourceWithActualData, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            _data.SetPythonEngine(new Mock<I_IPy4C>().Object, "Name");
        }
        private void SetupActivationDataSignalResponse()
        {
            var ahatTable = CreateSampleDataTable();
            for (int i = 0; i < 10; i++)
                ahatTable.Rows.Add(i, i * .25, i + .1);
            DataSource sourceWithActualData = SetupSampleDataSource(ahatTable);
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            _data.SetSource(sourceWithActualData, myFlaws, myMetaDatas, myResponses, mySpecIDs);
            _data.SetPythonEngine(new Mock<I_IPy4C>().Object, "Name");
        }
        private void AssertFlawResponseDataNotUpdated()
        {
            Assert.That(_data.HMAnalysisObject.Flaws.Count, Is.Zero);
            Assert.That(_data.AHATAnalysisObject.Flaws.Count, Is.Zero);
            Assert.IsFalse(_data.HMAnalysisObject.Responses.ContainsKey("Responses"));
            Assert.IsFalse(_data.AHATAnalysisObject.Responses.ContainsKey("Responses"));
        }
        /// Tests for SetPythonEngine(I_IPy4C myPy, string myAnalysisName)
        
        [Test]
        public void SetPythonEngine_IP4yCArgumentIsNull_NoAnalysisObjectCreated()
        {
            //Arrange
            //Act
            _data.SetPythonEngine(null, string.Empty);
            //Assert
            Assert.That(_data.HMAnalysisObject, Is.Null);
            Assert.That(_data.AHATAnalysisObject, Is.Null);
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.AHat)]
        [TestCase(AnalysisDataTypeEnum.HitMiss)]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void SetPythonEngine_IP4yCNotNullAndHMAndAHATObjectsNotNull_NoAnalysisObjectCreated(AnalysisDataTypeEnum analysisDataType)
        {
            //Arrange
            SetupIP4yC();
            _data.HMAnalysisObject = new HMAnalysisObject("myName");
            _data.AHATAnalysisObject = new AHatAnalysisObject("myName");
            _data.DataType = analysisDataType;
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Never);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Never);
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void SetPythonEngine_IP4yCNotNullAndInvalidAnalysisDataTypePresent_NoAnalysisObjectCreated(AnalysisDataTypeEnum analysisDataType)
        {
            //Arrange
            SetupIP4yC();
            _data.DataType = analysisDataType;
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Never);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Never);
        }
        [Test]
        public void SetPythonEngine_IP4yCNotNullAndAnalysisDataTypeHitMissButHMAnalysisObjectIsNotNull_NoAnalysisObjectCreated()
        {
            //Arrange
            SetupIP4yC();
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.HMAnalysisObject = new HMAnalysisObject("myName");
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Never);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Never);
        }
        [Test]
        public void SetPythonEngine_IP4yCNotNullAndAnalysisDataTypeAHatButAHatAnalysisObjectIsNotNull_NoAnalysisObjectCreated()
        {
            //Arrange
            SetupIP4yC();
            _data.DataType = AnalysisDataTypeEnum.AHat;
            _data.AHATAnalysisObject = new AHatAnalysisObject("myName");
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Never);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Never);
        }
        [Test]
        public void SetPythonEngine_IP4yCNotNullAndAnalysisDataTypeHitMiss_HMAnalysisCreated()
        {
            //Arrange
            SetupIP4yC();
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Once);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Never);
            Assert.That(_data.HMAnalysisObject, Is.Not.Null);
        }
        [Test]
        public void SetPythonEngine_IP4yCNotNullAndAnalysisDataTypeAHat_AHatAnalysisCreated()
        {
            //Arrange
            SetupIP4yC();
            _data.DataType = AnalysisDataTypeEnum.AHat;
            //Act
            _data.SetPythonEngine(_python.Object, string.Empty);
            //Assert
            _python.Verify(p => p.HitMissAnalsysis(It.IsAny<string>()), Times.Never);
            _python.Verify(p => p.AHatAnalysis(It.IsAny<string>()), Times.Once);
            Assert.That(_data.AHATAnalysisObject, Is.Not.Null);
        }
        private void SetupIP4yC()
        {
            _python = new Mock<I_IPy4C>();
            _python.Setup(p => p.HitMissAnalsysis(It.IsAny<string>())).Returns(new HMAnalysisObject("myName"));
            _python.Setup(p => p.AHatAnalysis(It.IsAny<string>())).Returns(new AHatAnalysisObject("myName"));
        }


        /// Tests For the function : UpdateOutput(RCalculationType myCalculationType,
        /// IUpdateOutputForAHatData updateOutputForAHatDataIn = null,
        /// IUpdateOutputForHitMissData updateOutputForHitMissDataIn=null)
        DataTable _fitResidualsTable;
        DataTable _residualUncensoredTable;
        DataTable _residualRawTable;
        DataTable _residualCensoredTable;
        DataTable _residualFullCensoredTable;
        DataTable _residualPartialCensoredTable;
        DataTable _podCurveTable;
        DataTable _podCurveTableAll;
        DataTable _thresholdPlotTable;
        DataTable _thresholdPlotTable_All;
        DataTable _normalityTable;
        DataTable _normalityCurveTable;
        [Test]
        public void UpdateOutput_DataTypeIsAHatAndRCalculationTypeIsFull_AllTablesUpdated()
        {
            //Arrange
            Mock<IUpdateOutputForAHatData> updateoutputForAHatData = new Mock<IUpdateOutputForAHatData>();
            _data.DataType = AnalysisDataTypeEnum.AHat;
            SetupTableVariables();
            //Act
            _data.UpdateOutput(RCalculationType.Full, updateoutputForAHatData.Object);
            //Assert
            updateoutputForAHatData.Verify(upahat => upahat.UpdatePODCurveTable(ref _podCurveTable), Times.Once);
            updateoutputForAHatData.Verify(upahat => upahat.UpdatePODCurveAllTable(ref _podCurveTableAll), Times.Once);
            VerifyAllButPODCurveTables(updateoutputForAHatData, Times.Once);
        }
        [Test]
        public void UpdateOutput_DataTypeIsAHatAndRCalculationTypeIsThresholdChange_OnlyPODTablesUpdated()
        {
            //Arrange
            Mock<IUpdateOutputForAHatData> updateoutputForAHatData = new Mock<IUpdateOutputForAHatData>();
            _data.DataType = AnalysisDataTypeEnum.AHat;
            SetupTableVariables();
            //Act
            _data.UpdateOutput(RCalculationType.ThresholdChange, updateoutputForAHatData.Object);
            //Assert
            updateoutputForAHatData.Verify(upahat => upahat.UpdatePODCurveTable(ref _podCurveTable), Times.Once);
            updateoutputForAHatData.Verify(upahat => upahat.UpdatePODCurveAllTable(ref _podCurveTableAll), Times.Once);
            //Make sure these aren't executed
            VerifyAllButPODCurveTables(updateoutputForAHatData, Times.Never);
        }
        private void SetupTableVariables()
        {
            _fitResidualsTable = _data.FitResidualsTable;
            _residualUncensoredTable = _data.ResidualUncensoredTable;
            _residualRawTable = _data.ResidualRawTable;
            _residualCensoredTable = _data.ResidualCensoredTable;
            _residualFullCensoredTable = _data.ResidualFullCensoredTable;
            _residualPartialCensoredTable = _data.ResidualPartialCensoredTable;
            _podCurveTable = _data.PodCurveTable;
            _podCurveTableAll = _data.PodCurveTable_All;
            _thresholdPlotTable = _data.ThresholdPlotTable;
            _thresholdPlotTable_All = _data.ThresholdPlotTable_All;
            _normalityTable = _data.NormalityTable;
            _normalityCurveTable = _data.NormalityCurveTable;
        }
        private void VerifyAllButPODCurveTables(Mock<IUpdateOutputForAHatData> updateoutputForAHatData, Func<Times> times)
        {
            updateoutputForAHatData.Verify(upahat => upahat.UpdateFitResidualsTable(ref _fitResidualsTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateResidualUncensoredTable(ref _residualUncensoredTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateResidualRawTable(ref _residualRawTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateResidualCensoredTable(ref _residualCensoredTable, _residualRawTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateResidualFullCensoredTable(ref _residualFullCensoredTable, _residualRawTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateResidualPartialCensoredTable(ref _residualPartialCensoredTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateThresholdCurveTable(ref _thresholdPlotTable), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateThresholdCurveTableAll(ref _thresholdPlotTable_All), times);
            updateoutputForAHatData.Verify(upahat => upahat.UpdateNormalityTable(ref _normalityTable, ref _normalityCurveTable), times);
        }
        //Calculation type should have no effect
        [Test]
        [TestCase(RCalculationType.Full)]
        [TestCase(RCalculationType.None)]
        [TestCase(RCalculationType.ThresholdChange)]
        public void UpdateOutput_DataTypeIsHitMiss_HitMissTablesUpdated(RCalculationType calcType)
        {
            //Arrange
            Mock<IUpdateOutputForHitMissData> updateoutputForHitMissData = new Mock<IUpdateOutputForHitMissData>();
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            var originalData = _data.OriginalData;
            var flawCount = _data.FlawCount;
            var podCurveTable = _data.PodCurveTable;
            var residualUncensoredTable = _data.ResidualUncensoredTable;
            var residualPartialCensoredTable = _data.ResidualPartialCensoredTable;
            var iterationsTable = _data.IterationsTable;
            //Act
            _data.UpdateOutput(calcType, null, updateoutputForHitMissData.Object);
            //Assert
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdateOriginalData(ref originalData));
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdateTotalFlawCount(ref flawCount));
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdatePODCurveTable(ref podCurveTable));
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdateResidualUncensoredTable(ref residualUncensoredTable));
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdateResidualPartialUncensoredTable(ref residualPartialCensoredTable));
            updateoutputForHitMissData.Verify(upahitmiss => upahitmiss.UpdateIterationsTable(ref iterationsTable));
        }
        /// Tests ChangeTableColumnNames(DataTable myTable, List<string> myNewNames)
        [Test]

        public void ChangeTableColumnNames_myTableIsNull_ReturnsEmptyStringOfOldNames()
        {
            //Arrange
            //Act
            var result=_data.ChangeTableColumnNames(null, new List<string>() { "MyNewName", "MyOtherNewName", "MyFinalNewName" });
            //Assert
            Assert.That(result.Count, Is.Zero);
        }
        [Test]
        public void ChangeTableColumnNames_MyTableHasNoColumns_ReturnsEmptyStringOfOldNames()
        {
            //Arrange
            _table = new DataTable();
            //Act
            var result = _data.ChangeTableColumnNames(_table, new List<string>() { "MyNewName", "MyOtherNewName", "MyFinalNewName"});
            //Assert
            Assert.That(result.Count, Is.Zero);
        }
        [Test]
        public void ChangeTableColumnNames_MyTableHasColumnsEqualToNewNames_ReturnsStringOfOldNames()
        {
            //Arrange
            //Act
            var result = _data.ChangeTableColumnNames(_table, new List<string>() { "MyNewName", "MyOtherNewName", "MyFinalNewName" });
            //Assert
            AssertOldColumnNames(result);
        }
        [Test]
        public void ChangeTableColumnNames_MyTableHasColumnsEqualLessNewNames_ReturnsStringOfOldNames()
        {
            //Arrange
            //Act
            var result = _data.ChangeTableColumnNames(_table, new List<string>() { "MyNewName", "MyOtherNewName", "MyFinalNewName", "OneExtraFinalNewName" });
            //Assert
            AssertOldColumnNames(result);
        }
        private void AssertOldColumnNames(List<string> result)
        {
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Contains("Column1"));
            Assert.That(result.Contains("Column2"));
            Assert.That(result.Contains("Column3"));
        }

        /// tests for the WriteToExcel(ExcelExport myWriter, string myAnalysisName, string myWorksheetName, bool myPartOfProject = true,
        /// IExcelWriterControl excelWriteControlIn = null) function
        [Test]
        public void WriteToExcel_AnalysisTypeHitMiss_ExecutesTheWriteIterationsToExcelTable()
        {
            //Arrange
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            //Act
            _data.WriteToExcel(_excelExport.Object, "AnalysisName", "WorksheetName", true, _excelWriterControl.Object);
            //Assert

            _excelWriterControl.Verify(ewc => ewc.WriteIterationsToExcel(_data, It.IsAny<DataTable>()), Times.Exactly(1));
            _excelWriterControl.Verify(ewc => ewc.WritePODThresholdToExcel(_data, It.IsAny<DataTable>()), Times.Never);
            AssertGeneralWriteToExcelTables();

        }
        [Test]
        public void WriteToExcel_AnalysisTypeAHat_ExecutesTheWritePODThresholdToExcelTable()
        {
            //Arrange
            _data.DataType = AnalysisDataTypeEnum.AHat;
            //Act
            _data.WriteToExcel(_excelExport.Object, "AnalysisName", "WorksheetName", true, _excelWriterControl.Object);
            //Assert

            _excelWriterControl.Verify(ewc => ewc.WriteIterationsToExcel(_data, It.IsAny<DataTable>()), Times.Never);
            _excelWriterControl.Verify(ewc => ewc.WritePODThresholdToExcel(_data, It.IsAny<DataTable>()), Times.Exactly(1));
            AssertGeneralWriteToExcelTables();
        }
        private void AssertGeneralWriteToExcelTables()
        {
            _excelWriterControl.Verify(ewc => ewc.WriteResidualsToExcel(_data, It.IsAny<DataTable>()), Times.Exactly(1));
            _excelWriterControl.Verify(ewc => ewc.WritePODToExcel(_data, It.IsAny<int>()), Times.Exactly(1));
            _excelWriterControl.Verify(ewc => ewc.WriteRemovedPointsToExcel(_data, It.IsAny<DataTable>(), It.IsAny<DataTable>(),
            It.IsAny<DataTable>()), Times.Exactly(1));
        }
        /// Tests for the AdditionalWorksheet1Name getter
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void AdditionalWorksheet1Name_DataTypeIsInvalid_ReturnsNotApplicable(AnalysisDataTypeEnum datatype)
        {
            //Arrange
            _data.DataType = datatype;
            //Act
            var result = _data.AdditionalWorksheet1Name;
            //Assert
            Assert.That(result, Is.EqualTo(Globals.NotApplicable));
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.AHat, "Threshold")]
        [TestCase(AnalysisDataTypeEnum.HitMiss, "Solver")]
        public void AdditionalWorksheet1Name_DataTypeIsValid_ReturnsAppropriateString(AnalysisDataTypeEnum datatype, string expectedName)
        {
            //Arrange
            _data.DataType = datatype;
            //Act
            var result = _data.AdditionalWorksheet1Name;
            //Assert
            Assert.That(result, Is.EqualTo(expectedName));
        }
        /// Tests for the UncensoredFlawRangeMin getter
        [Test]
        [TestCase(TransformTypeEnum.Linear , 1.0)]
        [TestCase(TransformTypeEnum.Log, 4.0)]
        [TestCase(TransformTypeEnum.Inverse, 7.0)]
        public void UncensoredFlawRangeMin_DataTypeIsHitMiss_ReturnsMinBasedOnTransformType(TransformTypeEnum transform, double min)
        {
            //Arrange
            _data.FlawTransform = transform;
            HMAnalysisObject hmAnalysisObject = SetupFlawsAtAllTransforms(new HMAnalysisObject("HitMissName"));
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.HMAnalysisObject = hmAnalysisObject;
            //Act
            var result = _data.UncensoredFlawRangeMin;
            //Assert
            Assert.That(result, Is.EqualTo(min));

        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 1.0)]
        [TestCase(TransformTypeEnum.Log, 4.0)]
        public void UncensoredFlawRangeMin_DataTypeIsAHat_ReturnsMinBasedOnTransformType(TransformTypeEnum transform, double min)
        {
            //Arrange
            _data.FlawTransform = transform;
            AHatAnalysisObject ahatAnalysisObject = SetupFlawsAtAllTransformsAHAT(new AHatAnalysisObject("AHatName"));
            _data.DataType = AnalysisDataTypeEnum.AHat;
            _data.AHATAnalysisObject = ahatAnalysisObject;
            //Act
            var result = _data.UncensoredFlawRangeMin;
            //Assert
            Assert.That(result, Is.EqualTo(min));

        }
        private HMAnalysisObject SetupFlawsAtAllTransforms(HMAnalysisObject hmAnalysisObject)
        {
            hmAnalysisObject.Flaws_All = new List<double>() { 1.0, 2.0, 3.0 };
            hmAnalysisObject.LogFlaws_All = new List<double>() { 4.0, 5.0, 6.0 };
            hmAnalysisObject.InverseFlaws_All = new List<double>() { 7.0, 8.0, 9.0 };
            return hmAnalysisObject;
        }
        private AHatAnalysisObject SetupFlawsAtAllTransformsAHAT(AHatAnalysisObject ahatAnalysisObject)
        {
            ahatAnalysisObject.Flaws_All = new List<double>() { 1.0, 2.0, 3.0 };
            ahatAnalysisObject.LogFlaws_All = new List<double>() { 4.0, 5.0, 6.0 };
            ahatAnalysisObject.InverseFlaws_All = new List<double>() { 7.0, 8.0, 9.0 };
            return ahatAnalysisObject;
        }
        /// Tests for the FlawRangeMin getter
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        [TestCase(AnalysisDataTypeEnum.HitMiss)]
        [TestCase(AnalysisDataTypeEnum.AHat)]
        public void FlawRangeMin_DataTypePassedButFlawsForbothHitMissAndAHatAreEmpty_ReturnsNaN(AnalysisDataTypeEnum dataType)
        {
            //Arrange
            _data.DataType = dataType;
            SetupAHatAndHMAnalysisObjects();
            //Act
            var result = _data.FlawRangeMin;
            //Assert
            Assert.That(result, Is.EqualTo(double.NaN));
        }
        [Test]
        public void FlawRangeMin_DataTypeIsHitMissAndFlawsCountIsGreaterThan0_ReturnsMinFlaw()
        {
            //Arrange
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            SetupAHatAndHMAnalysisObjects();
            _data.HMAnalysisObject.Flaws = new List<double>() { 1.0, 2.0, 3.0 };
            //Act
            var result = _data.FlawRangeMin;
            //Assert
            Assert.That(result, Is.EqualTo(1.0));
        }
        [Test]
        public void FlawRangeMin_DataTypeIsAHatAndFlawsCountIsGreaterThan0_ReturnsMinFlaw()
        {
            //Arrange
            _data.DataType = AnalysisDataTypeEnum.AHat;
            SetupAHatAndHMAnalysisObjects();
            _data.AHATAnalysisObject.Flaws = new List<double>() { 1.0, 2.0, 3.0 };
            //Act
            var result = _data.FlawRangeMin;
            //Assert
            Assert.That(result, Is.EqualTo(1.0));
        }
        private void SetupAHatAndHMAnalysisObjects()
        {
            _data.HMAnalysisObject = new HMAnalysisObject("HitMissName");
            _data.AHATAnalysisObject = new AHatAnalysisObject("AHatName");
        }
        /// Tests for the UncensoredFlawRangeMax getter
        [Test]
        [TestCase(TransformTypeEnum.Linear, 3.0)]
        [TestCase(TransformTypeEnum.Log, 6.0)]
        [TestCase(TransformTypeEnum.Inverse, 9.0)]
        public void UncensoredFlawRangeMax_DataTypeIsHitMiss_ReturnsMaxBasedOnTransformType(TransformTypeEnum transform, double min)
        {
            //Arrange
            _data.FlawTransform = transform;
            HMAnalysisObject hmAnalysisObject = SetupFlawsAtAllTransforms(new HMAnalysisObject("HitMissName"));
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.HMAnalysisObject = hmAnalysisObject;
            //Act
            var result = _data.UncensoredFlawRangeMax;
            //Assert
            Assert.That(result, Is.EqualTo(min));

        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 3.0)]
        [TestCase(TransformTypeEnum.Log, 6.0)]
        public void UncensoredFlawRangeMax_DataTypeIsAHat_ReturnsMaxBasedOnTransformType(TransformTypeEnum transform, double min)
        {
            //Arrange
            _data.FlawTransform = transform;
            AHatAnalysisObject ahatAnalysisObject = SetupFlawsAtAllTransformsAHAT(new AHatAnalysisObject("AHatName"));
            _data.DataType = AnalysisDataTypeEnum.AHat;
            _data.AHATAnalysisObject = ahatAnalysisObject;
            //Act
            var result = _data.UncensoredFlawRangeMax;
            //Assert
            Assert.That(result, Is.EqualTo(min));

        }
        /// Tests for InvertTransformedFlaw(double myValue)
        [Test]
        [TestCase(TransformTypeEnum.Linear)]
        [TestCase(TransformTypeEnum.Log)]
        [TestCase(TransformTypeEnum.Exponetial)]
        [TestCase(TransformTypeEnum.Inverse)]
        [TestCase(TransformTypeEnum.BoxCox)]
        [TestCase(TransformTypeEnum.None)]
        public void InvertTransformedFlaw_BothAHatandHMAnalysisObjectIsNull_ReturnsTheSameValue(TransformTypeEnum transform)
        {
            //Arrange
            SetupPythonMock();
            _data.FlawTransform = transform;
            //Act
            var result = _data.InvertTransformedFlaw(1.0);
            //Assert
            Assert.That(result, Is.EqualTo(1));
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Never);
        }
        /// The log transform is in a separate test since it requires Math.Exp
        [Test]
        [TestCase(1, 2.0)]
        [TestCase(3, 1.0/2.0)]
        [TestCase(4, 2.0)]
        [TestCase(5, 3.0)]
        [TestCase(6, 2.0)]
        public void InvertTransformedFlaw_AHatAnalysisObjectNotNull_ReturnsTransformedValue(int inputTransform, double expectedTransformValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(inputTransform);
            _data.AHATAnalysisObject = new AHatAnalysisObject("AnalysisName") { Lambda = 1.0 };
            //Act
            var result = _data.InvertTransformedFlaw(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            Assert.That(result, Is.EqualTo(expectedTransformValue));
        }
        [Test]
        public void InvertTransformedFlaw_AHatAnalysisObjectNotNullAndTransformIsLog_ReturnsTransformedValue()
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(2);
            _data.AHATAnalysisObject = new AHatAnalysisObject("AnalysisName");
            //Act
            var result = _data.InvertTransformedFlaw(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            Assert.That(result, Is.EqualTo(Math.Exp(2)));
        }
        // BoxCox is never passed in for hitmiss (case = 5)
        [Test]
        [TestCase(1, 2.0)]
        [TestCase(3, 1.0 / 2.0)]
        [TestCase(4, 2.0)]
        [TestCase(6, 2.0)]
        public void InvertTransformedFlaw_HMAnalysisObjectNotNull_ReturnsTransformedValue(int inputTransform, double expectedTransformValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(inputTransform);
            _data.HMAnalysisObject = new HMAnalysisObject("AnalysisName");
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            //Act
            var result = _data.InvertTransformedFlaw(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            Assert.That(result, Is.EqualTo(expectedTransformValue));
        }

        private void SetupPythonMock()
        {
            _python = new Mock<I_IPy4C>();
            _data.SetPythonEngine(_python.Object, "AnalysisName");
        }
        /// tests for InvertTransformedResponse(double myValue)
        /// The log transform is in a separate test since it requires Math.Exp
        [Test]
        [TestCase(1, 2.0)]
        [TestCase(3, 1.0/2.0)]
        [TestCase(4, 2.0)]
        [TestCase(5, 3.0)]
        [TestCase(6, 2.0)]
        public void InvertTransformedResponse_AHatAnalysisObjectNotNull_ReturnsTransformedValue(int inputTransform, double expectedTransformValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(inputTransform);
            _data.AHATAnalysisObject = new AHatAnalysisObject("AnalysisName") { Lambda = 1.0 };
            //Act
            var result = _data.InvertTransformedResponse(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            Assert.That(result, Is.EqualTo(expectedTransformValue));
        }
        [Test]
        public void InvertTransformedResponse_AHatAnalysisObjectNotNullAndTransformIsLog_ReturnsTransformedValue()
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(2);
            _data.AHATAnalysisObject = new AHatAnalysisObject("AnalysisName");
            //Act
            var result = _data.InvertTransformedResponse(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            Assert.That(result, Is.EqualTo(Math.Exp(2)));
        }
        [Test]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void InvertTransformedResponse_HMAnalysisObjectNotNull_ReturnsTransformedValue(int inputTransform)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(inputTransform);
            _data.HMAnalysisObject = new HMAnalysisObject("AnalysisName");
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            //Act
            var result = _data.InvertTransformedResponse(2.0);
            //Assert
            _python.Verify(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Never);
            Assert.That(result, Is.EqualTo(2.0));
        }
        /// Tests for FlawCount getter
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void FlawCount_DataTypeInvalid_Returns0(AnalysisDataTypeEnum datatype)
        {
            // Arrange
            _data.DataType = datatype;
            //Act
            var result = _data.FlawCount;
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCount_DataTypeIsHitMiss_ReturnsTotalFlawCount()
        {
            // Arrange
            HMAnalysisObject hmAnalysisObject = new HMAnalysisObject("AnalysisName") { Flaws = new List<double>() { 1.0, 2.0, 3.0 } };
            UpdateOutputForHitMissData updateOutput = new UpdateOutputForHitMissData(hmAnalysisObject, new Mock<IMessageBoxWrap>().Object);
            _data.HMAnalysisObject = hmAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.UpdateOutput(RCalculationType.Full, null, updateOutput);
            // Act
            var result=_data.FlawCount;
            // Assert
            Assert.That(result, Is.EqualTo(3));
        }
        [Test]
        public void FlawCount_DataTypeIsAHatAndTablesAreNotNull_ReturnsSumOfResidualTables()
        {
            // Arrange
            SetupResidTable();
            AHatAnalysisObject ahatAnalysisObject = CreateFakeAHatObject(_table, _table.Copy());
            SetupUpdateOutputDataAHAT(ahatAnalysisObject);
            // Act
            var result = _data.FlawCount;
            // Assert
            Assert.That(result, Is.EqualTo(10));
        }
        [Test]
        public void FlawCount_DataTypeIsAHatAndBothTablesAreNull_ReturnsSumOfResidualTables()
        {
            // Arrange
            SetupResidTable();
            AHatAnalysisObject ahatAnalysisObject = CreateFakeAHatObject(null, null);
            _data.AHATAnalysisObject = ahatAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.AHat;
            // Act
            var result = _data.FlawCount;
            // Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCount_DataTypeIsAHatAndUncensoredTableIsNull_ReturnsSumOfResidualTables()
        {
            // Arrange
            SetupResidTable();
            AHatAnalysisObject ahatAnalysisObject = CreateFakeAHatObject(_table, null);
            SetupUpdateOutputDataAHAT(ahatAnalysisObject);
            // Act
            var result = _data.FlawCount;
            // Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCount_DataTypeIsAHatAndCensoredTableIsNull_ReturnsSumOfResidualTables()
        {
            // Arrange
            SetupResidTable();
            AHatAnalysisObject ahatAnalysisObject = CreateFakeAHatObject(null, _table);
            SetupUpdateOutputDataAHAT(ahatAnalysisObject);
            // Act
            var result = _data.FlawCount;
            // Assert
            Assert.That(result, Is.Zero);
        }
        private AHatAnalysisObject CreateFakeAHatObject(DataTable table1, DataTable table2)
        {
            AHatAnalysisObject ahatAnalysisObject = new AHatAnalysisObject("AnalysisName")
            {
                Flaws = new List<double>() { 1.0, 2.0, 3.0 },
                AHatResultsResidUncensored = table1,
                AHatResultsResid = table2,
                //Censor one of the points to ensure that all the flaws are still being added up
                FlawsCensored = new List<double>() { 2.0 }
            };
            return ahatAnalysisObject;
        }
        private void SetupUpdateOutputDataAHAT(AHatAnalysisObject ahatAnalysisObject)
        {
            UpdateOutputForAHatData updateOutput = new UpdateOutputForAHatData(ahatAnalysisObject, new Mock<IMessageBoxWrap>().Object);
            _data.AHATAnalysisObject = ahatAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.AHat;
            _data.UpdateOutput(RCalculationType.Full, updateOutput);
        }
        private void SetupResidTable()
        {
            _table.Columns.Add("Column4");
            _table.Columns["Column1"].ColumnName = "flaw";
            _table.Columns["Column2"].ColumnName = "y";
            for (int i = 1; i < 11; i++)
                _table.Rows.Add(i, i * .25, i + 1, i + 2);
        }

        /// Tests for FlawCountUnique getter
        /// This getter is only really used for hitmiss
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void FlawCountUnique_DataTypeNotValid_Returns0(AnalysisDataTypeEnum datatype)
        {
            //Arrange
            _data.DataType = datatype;
            //Act
            var result = _data.FlawCountUnique;
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCountUnique_DataTypeAHat_Returns0()
        {
            //Arrange
            SetupResidTable();
            AHatAnalysisObject ahatAnalysisObject = CreateFakeAHatObject(null, null);
            UpdateOutputForAHatData updateOutput = new UpdateOutputForAHatData(ahatAnalysisObject, new Mock<IMessageBoxWrap>().Object);
            _data.AHATAnalysisObject = ahatAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.AHat;
            _data.UpdateOutput(RCalculationType.Full, updateOutput);
            //Act
            var result = _data.FlawCountUnique;
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCountUnique_DataTypeIsHitMissAndResidualTableIsNull_Returns0()
        {
            //Arrange
            SetupResidTable();
            HMAnalysisObject hmAnalysisObject = new HMAnalysisObject("AnalysisName") { 
                Flaws = new List<double>() { 1.0, 2.0, 3.0 },
                ResidualTable = null
            };
            UpdateOutputForHitMissData updateOutput = new UpdateOutputForHitMissData(hmAnalysisObject, new Mock<IMessageBoxWrap>().Object);
            _data.HMAnalysisObject = hmAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.UpdateOutput(RCalculationType.Full, null, updateOutput);
            //Act
            var result = _data.FlawCountUnique;
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        public void FlawCountUnique_DataTypeIsHitMissAndResidualTableIsNotNull_ReturnsCountOfRows()
        {
            //Arrange
            SetupResidTable();
            HMAnalysisObject hmAnalysisObject = new HMAnalysisObject("AnalysisName") { 
                Flaws = new List<double>() { 1.0, 2.0, 3.0 },
                ResidualTable = _table,  
            };
            UpdateOutputForHitMissData updateOutput = new UpdateOutputForHitMissData(hmAnalysisObject, new Mock<IMessageBoxWrap>().Object);
            _data.HMAnalysisObject = hmAnalysisObject;
            _data.DataType = AnalysisDataTypeEnum.HitMiss;
            _data.UpdateOutput(RCalculationType.Full, null, updateOutput);
            //Act
            var result = _data.FlawCountUnique;
            //Assert
            Assert.That(result, Is.EqualTo(10));
        }

        /// Skipping buffered ranges and minmax functions for now
        /// 

        /// Tests for TransformAValue(double myValue, int transform) function
        [Test]
        [TestCase(1, 2.0)]
        [TestCase(3, 0.5)]
        [TestCase(4, 2.0)]
        [TestCase(6, 2.0)]
        public void TransformAValue_LinearOrInverseTransformPassed_ReturnsTransformedValue(int transform, double expectedValue)
        {
            //Arrange
            //Act
            var result = _data.TransformAValue(2.0, transform);
            //Assert
            Assert.That(result, Is.EqualTo(expectedValue));
        }
        [Test]
        public void TransformAValue_LogTransformPassed_ReturnsTransformedValue()
        {
            //Arrange
            //Act
            var result = _data.TransformAValue(Math.E, 2);
            //Assert
            Assert.That(result, Is.EqualTo(1));
        }
        [Test]
        public void TransformAValue_BoxCoxTransformPassed_ReturnsTransformedValue()
        {
            //Arrange
            _data.AHATAnalysisObject = new AHatAnalysisObject("AnalysisName") { Lambda = 2.0 };
            //Act
            var result = _data.TransformAValue(2.0, 5);
            //Assert
            Assert.That(result, Is.EqualTo(1.5));
        }

        /// Tests for TransformBackAValue(double myValue, int transform) function
        [Test]
        [TestCase(1, 1.0 / 2.0, 1.0 / 2.0)]
        [TestCase(3, 1.0 / 2.0, 2.0)]
        public void TransformBackAValue_LinearOrInverseTransformPassed_ReturnsTransformedBackValue(int transform, double transformedValue, double expectedBackValue)
        {
            //Arrange
            //Act
            var result = _data.TransformBackAValue(transformedValue, transform);
            //Assert
            Assert.That(result, Is.EqualTo(expectedBackValue));
        }
        [Test]
        public void TransformBackAValue_TransformTypeIsLog_ReturnsTransformedBackValue()
        {
            //Arrange

            //Act
            var result = _data.TransformBackAValue(2.0, 2);
            //Assert
            Assert.That(result, Is.EqualTo(Math.Exp(2.0)));
        }
        [Test]
        public void TransformBackAValue_TransformIsBoxCox_ReturnsTransformedBackValue()
        {
            //Arrange
            Mock<ITransformBackLambdaControl> transformBackLambdaControl = new Mock<ITransformBackLambdaControl>();
            transformBackLambdaControl.Setup(tblc => tblc.TransformBackLambda(It.IsAny<double>())).Returns(-1.0);
            _data.TransformBackLambda = transformBackLambdaControl.Object;
            //Act
            var result = _data.TransformBackAValue(2.0, 5);
            //Assert
            Assert.That(result, Is.EqualTo(-1.0));
        }
        [Test]
        [TestCase(4, .5)]
        [TestCase(4, 1.0)]
        [TestCase(6, .5)]
        [TestCase(6, 1.0)]
        public void TransformBackAValue_InvalidTransformPassed_ReturnsTheSameValue(int transform, double inputValue)
        {
            //Act
            var result = _data.TransformBackAValue(inputValue, transform);
            //Assert
            Assert.That(result, Is.EqualTo(inputValue));
        }

        /// Tests for TransformValueForXAxis
        [Test]
        [TestCase(TransformTypeEnum.Log, 0.0)]
        [TestCase(TransformTypeEnum.Log, -1.0)]
        [TestCase(TransformTypeEnum.Inverse, 0.0)]
        [TestCase(TransformTypeEnum.Inverse, -1.0)]
        public void TransformValueForXAxis_ValueIsZeroOrLessAndTransformIsLogOrInverse_Returns0(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            _data.FlawTransform = transform;
            //Act
            var result=_data.TransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 0.0)]
        [TestCase(TransformTypeEnum.Linear, -1.0)]
        [TestCase(TransformTypeEnum.Exponetial, 0.0)]
        [TestCase(TransformTypeEnum.Exponetial, -1.0)]
        [TestCase(TransformTypeEnum.BoxCox, 0.0)]
        [TestCase(TransformTypeEnum.BoxCox, -1.0)]
        [TestCase(TransformTypeEnum.Custom, 0.0)]
        [TestCase(TransformTypeEnum.Custom, -1.0)]
        [TestCase(TransformTypeEnum.None, 0.0)]
        [TestCase(TransformTypeEnum.None, -1.0)]
        [TestCase(TransformTypeEnum.Log, 1.0)]
        [TestCase(TransformTypeEnum.Log, 1.0)]
        [TestCase(TransformTypeEnum.Inverse, 1.0)]
        [TestCase(TransformTypeEnum.Inverse, 1.0)]
        public void TransformValueForXAxis_MyValueIsGreaterThan0OrTransformIsNotLogOrInverse_ReturnsTransformValue(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(1); //Effectively makes the transform linear
            _data.FlawTransform=transform;
            //Act
            var result=_data.TransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(inputValue));
            _python.Verify(p => p.TransformEnumToInt(transform));
        }
        [Test]
        [TestCase(TransformTypeEnum.Log, 0.0)]
        [TestCase(TransformTypeEnum.Log, -1.0)]
        [TestCase(TransformTypeEnum.Inverse, 0.0)]
        [TestCase(TransformTypeEnum.Inverse, -1.0)]
        /// Tests for TransformValueForYAxis
        public void TransformValueForYAxis_ValueIsZeroOrLessAndTransformIsLogOrInverse_Returns0(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            _data.ResponseTransform = transform;
            //Act
            var result = _data.TransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 0.0)]
        [TestCase(TransformTypeEnum.Linear, -1.0)]
        [TestCase(TransformTypeEnum.Exponetial, 0.0)]
        [TestCase(TransformTypeEnum.Exponetial, -1.0)]
        [TestCase(TransformTypeEnum.BoxCox, 0.0)]
        [TestCase(TransformTypeEnum.BoxCox, -1.0)]
        [TestCase(TransformTypeEnum.Custom, 0.0)]
        [TestCase(TransformTypeEnum.Custom, -1.0)]
        [TestCase(TransformTypeEnum.None, 0.0)]
        [TestCase(TransformTypeEnum.None, -1.0)]
        [TestCase(TransformTypeEnum.Log, 1.0)]
        [TestCase(TransformTypeEnum.Inverse, 1.0)]
        public void TransformValueForYAxis_MyValueIsGreaterThan0OrTransformIsNotLogOrInverse_ReturnsTransformValue(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(1); //Effectively makes the transform linear
            _data.ResponseTransform = transform;
            //Act
            var result = _data.TransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(inputValue));
            _python.Verify(p => p.TransformEnumToInt(transform));
        }
        /// Tests for InvertTransformValueForXAxis
        [TestCase(TransformTypeEnum.Inverse, 0.0)]
        [Test]
        public void InvertTransformValueForXAxis_ValueIsZeroOrLessAndTransformIsLogOrInverse_Returns0(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            _data.FlawTransform = transform;
            //Act
            var result = _data.InvertTransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 0.0)]
        [TestCase(TransformTypeEnum.Linear, -1.0)]
        [TestCase(TransformTypeEnum.Log, 0.0)]
        [TestCase(TransformTypeEnum.Log, -1.0)]
        [TestCase(TransformTypeEnum.Exponetial, 0.0)]
        [TestCase(TransformTypeEnum.Exponetial, -1.0)]
        [TestCase(TransformTypeEnum.BoxCox, 0.0)]
        [TestCase(TransformTypeEnum.BoxCox, -1.0)]
        [TestCase(TransformTypeEnum.Custom, 0.0)]
        [TestCase(TransformTypeEnum.Custom, -1.0)]
        [TestCase(TransformTypeEnum.None, 0.0)]
        [TestCase(TransformTypeEnum.Inverse, -1.0)]
        [TestCase(TransformTypeEnum.Inverse, 1.0)]
        public void InvertTransformValueForXAxis_MyValueIsGreaterThan0OrTransformIsNotLogOrInverse_ReturnsTransformBackValue(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(1); //Effectively makes the transform linear
            _data.FlawTransform = transform;
            //Act
            var result = _data.InvertTransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(inputValue));
            _python.Verify(p => p.TransformEnumToInt(transform));
        }
        /// Tests for InvertTransformValueForXAxis
        [Test]
        [TestCase(TransformTypeEnum.Inverse, 0.0)]
        public void InvertTransformValueForYAxis_ValueIsZeroOrLessAndTransformIsLogOrInverse_Returns0(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            _data.ResponseTransform = transform;
            //Act
            var result = _data.InvertTransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.Zero);
        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, 0.0)]
        [TestCase(TransformTypeEnum.Linear, -1.0)]
        [TestCase(TransformTypeEnum.Log, 0.0)]
        [TestCase(TransformTypeEnum.Log, -1.0)]
        [TestCase(TransformTypeEnum.Exponetial, 0.0)]
        [TestCase(TransformTypeEnum.Exponetial, -1.0)]
        [TestCase(TransformTypeEnum.BoxCox, 0.0)]
        [TestCase(TransformTypeEnum.BoxCox, -1.0)]
        [TestCase(TransformTypeEnum.Custom, 0.0)]
        [TestCase(TransformTypeEnum.Custom, -1.0)]
        [TestCase(TransformTypeEnum.None, 0.0)]
        [TestCase(TransformTypeEnum.Inverse, -1.0)]
        [TestCase(TransformTypeEnum.Inverse, 1.0)]
        public void InvertTransformValueForYAxis_MyValueIsGreaterThan0OrTransformIsNotLogOrInverse_ReturnsTransformBackValue(TransformTypeEnum transform, double inputValue)
        {
            //Arrange
            SetupPythonMock();
            _python.Setup(p => p.TransformEnumToInt(It.IsAny<TransformTypeEnum>())).Returns(1); //Effectively makes the transform linear
            _data.ResponseTransform = transform;
            //Act
            var result = _data.InvertTransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(inputValue));
            _python.Verify(p => p.TransformEnumToInt(transform));
        }
        // Get FlawTransFormLabel tests
        [Test]
        [TestCase(TransformTypeEnum.Log, "ln(a)")]
        [TestCase(TransformTypeEnum.Linear, "{{a}}")]
        [TestCase(TransformTypeEnum.Exponetial, "e^a")]
        [TestCase(TransformTypeEnum.Inverse, "1/a")]
        [TestCase(TransformTypeEnum.None, "Custom")]
        [TestCase(TransformTypeEnum.Custom, "Custom")]
        public void FlawTransFormLabel_ValidTransform_ReturnsEquationStringFlaw(TransformTypeEnum flawTransform, string expectedString)
        {
            //Arrange
            _data.FlawTransform = flawTransform;
            //Act
            var result = _data.FlawTransFormLabel;
            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }
        // Get ResponseTransformLabel tests
        [Test]
        [TestCase(TransformTypeEnum.Log, "ln(ahat)")]
        [TestCase(TransformTypeEnum.Linear, "{{ahat}}")]
        [TestCase(TransformTypeEnum.Exponetial, "e^ahat")]
        [TestCase(TransformTypeEnum.Inverse, "1/ahat")]
        [TestCase(TransformTypeEnum.BoxCox, "[(ahat)^(lambda)-1]/lambda")]
        [TestCase(TransformTypeEnum.Custom, "Custom")]
        [TestCase(TransformTypeEnum.None, "Custom")]
        public void ResponseTransFormLabel_ValidTransform_ReturnsEquationStringFlaw(TransformTypeEnum responseResponse, string expectedString)
        {
            //Arrange
            _data.ResponseTransform = responseResponse;
            //Act
            var result = _data.ResponseTransformLabel;
            //Assert
            Assert.That(result, Is.EqualTo(expectedString));
        }
        /// Test for the UpdateSourceFromInfos(SourceInfo sourceInfo, ITableUpdaterFromInfos tableUpdaterFromInfosIn = null) function
        [Test]
        public void UpdateSourceFromInfos_ValidSourceInfoPassed_ExecutesUpdateTableFromInfosForbothFlawAndResponse()
        {
            //Arrange
            Mock<ITableUpdaterFromInfos> tableUpdater = new Mock<ITableUpdaterFromInfos>();
            //Act
            _data.UpdateSourceFromInfos(new SourceInfo("","",new DataSource(new DataTable(), new TableRange(), new TableRange(), new TableRange(), new TableRange())), tableUpdater.Object);
            //Assert
            tableUpdater.Verify(tu => tu.UpdateTableFromInfos(It.IsAny<SourceInfo>(), ColType.Flaw, It.IsAny<DataTable>(), It.IsAny<DataTable>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()));
            tableUpdater.Verify(tu => tu.UpdateTableFromInfos(It.IsAny<SourceInfo>(), ColType.Response, It.IsAny<DataTable>(), It.IsAny<DataTable>(), It.IsAny<List<string>>(), It.IsAny<List<string>>()));
        }
        /// GetUpdatedValue(ColType myType, string myExtColProperty, double currentValue, out double newValue)
        [Test]
        [TestCase(ColType.Flaw, ExtColProperty.Min, 1)]
        [TestCase(ColType.Flaw, ExtColProperty.Thresh,1)]
        [TestCase(ColType.Flaw, ExtColProperty.Max, 2)]
        public void GetUpdatedValue_FlawColumnTypeWithValidExtColProp_ReturnsTheNewValueAccordingly(ColType colType, string extColProp, double expectedValue)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            _data.GetUpdatedValue(colType, extColProp, .5, out double newValue);
            //Assert
            Assert.That(newValue, Is.EqualTo(expectedValue));
            availableFlawsTable.VerifyGet(aft => aft.Columns, Times.Once);
            availableResponsesTable.VerifyGet(aft => aft.Columns, Times.Never);
        }
        [Test]
        [TestCase(ColType.Response, ExtColProperty.Min, 1)]
        [TestCase(ColType.Response, ExtColProperty.Thresh, 1)]
        [TestCase(ColType.Response, ExtColProperty.Max, 2)]
        public void GetUpdatedValue_ResponseColumnTypeWithValidExtColProp_ReturnsTheNewValueAccordingly(ColType colType, string extColProp, double expectedValue)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            _data.GetUpdatedValue(colType, extColProp, .5, out double newValue);
            //Assert
            Assert.That(newValue, Is.EqualTo(expectedValue));
            availableFlawsTable.VerifyGet(aft => aft.Columns, Times.Never);
            availableResponsesTable.VerifyGet(aft => aft.Columns, Times.Once);
        }
        [Test]
        [TestCase(ColType.ID)]
        [TestCase(ColType.Meta)]
        public void GetUpdatedValue_InvalidColumnTypePassed_ThrowsArgumentException(ColType colType)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => _data.GetUpdatedValue(colType, It.IsAny<string>(), It.IsAny<double>(), out double newValue));
        }
        [Test]
        [TestCase(ColType.Flaw, "", 0.25)]
        [TestCase(ColType.Response, "", 1.1)]
        public void GetUpdatedValue_InvalidExtColPropertyPassed_ReturnsTheCurrentValueAndThrowsException(ColType colType, string extColProp, double expectedValue)
        {
            Assert.Throws<Exception>(() => _data.GetUpdatedValue(colType, extColProp, -1.0, out double newValue));
        }

        /// Tests for the GetNewValue(ColType myType, string myExtColProperty, out double newValue) function
        [Test]
        [TestCase(ColType.Flaw, ExtColProperty.Min, 1)]
        [TestCase(ColType.Flaw, ExtColProperty.Thresh, 1)]
        [TestCase(ColType.Flaw, ExtColProperty.Max, 2)]
        public void GetNewValue_FlawColumnTypeWithValidExtColProp_ReturnsTheNewValueAccordingly(ColType colType, string extColProp, double expectedValue)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            _data.GetNewValue(colType, extColProp, out double newValue);
            //Assert
            Assert.That(newValue, Is.EqualTo(expectedValue));
            availableFlawsTable.VerifyGet(aft => aft.Columns, Times.Once);
            availableResponsesTable.VerifyGet(aft => aft.Columns, Times.Never);
        }
        [Test]
        [TestCase(ColType.Response, ExtColProperty.Min, 1)]
        [TestCase(ColType.Response, ExtColProperty.Thresh, 1)]
        [TestCase(ColType.Response, ExtColProperty.Max, 2)]
        public void GetNewValue_ResponseColumnTypeWithValidExtColProp_ReturnsTheNewValueAccordingly(ColType colType, string extColProp, double expectedValue)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            _data.GetNewValue(colType, extColProp, out double newValue);
            //Assert
            Assert.That(newValue, Is.EqualTo(expectedValue));
            availableFlawsTable.VerifyGet(aft => aft.Columns, Times.Never);
            availableResponsesTable.VerifyGet(aft => aft.Columns, Times.Once);
        }
        [Test]
        [TestCase(ColType.ID)]
        [TestCase(ColType.Meta)]
        public void GetNewValue_InvalidColumnTypePassed_ThrowsArgumentException(ColType colType)
        {
            //Arrange
            AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable);
            //Act
            //Assert
            Assert.Throws<ArgumentException>(() => _data.GetNewValue(colType, It.IsAny<string>(), out double newValue));
        }
        [Test]
        [TestCase(ColType.Flaw, "", 0.25)]
        [TestCase(ColType.Response, "", 1.1)]
        public void GetNewValue_InvalidExtColPropertyPassed_ReturnsTheCurrentValueAndThrowsException(ColType colType, string extColProp, double expectedValue)
        {
            Assert.Throws<Exception>(() => _data.GetNewValue(colType, extColProp, out double newValue));
        }


        private void AssignExcelAndDataTableMocksToGetUpdatedValue(out Mock<IDataTableWrapper> availableFlawsTable, out Mock<IDataTableWrapper> availableResponsesTable)
        {
            Mock<IUpdaterExcelPropertyValue> updaterExcelProp = SetupValuesAddInExcelPropertyValue();
            availableFlawsTable = new Mock<IDataTableWrapper>();
            availableResponsesTable = new Mock<IDataTableWrapper>();
            availableFlawsTable.SetupGet(aft => aft.Columns).Returns(_table.Columns);
            availableResponsesTable.SetupGet(art => art.Columns).Returns(_table.Columns);
            _data.UpdaterExcelProp = updaterExcelProp.Object;
            _data.AvailableFlawsTable = availableFlawsTable.Object;
            _data.AvailableResponsesTable = availableResponsesTable.Object;
        }
        private Mock<IUpdaterExcelPropertyValue> SetupValuesAddInExcelPropertyValue()
        {
            Mock<IUpdaterExcelPropertyValue> updaterExcelProp = new Mock<IUpdaterExcelPropertyValue>();
            updaterExcelProp.Setup(uep => uep.GetUpdatedValue(ExtColProperty.Thresh, It.IsAny<double>(), It.IsAny<DataColumn>())).Returns(1); // GetUpdatedValue setup
            updaterExcelProp.Setup(uep => uep.GetUpdatedValue(ExtColProperty.Min, It.IsAny<double>(), It.IsAny<DataColumn>())).Returns(1);
            updaterExcelProp.Setup(uep => uep.GetUpdatedValue(ExtColProperty.Max, It.IsAny<double>(), It.IsAny<DataColumn>())).Returns(2);
            updaterExcelProp.Setup(uep => uep.GetNewValue(ExtColProperty.Thresh,  It.IsAny<DataColumn>())).Returns(1); // GetNewValue setup
            updaterExcelProp.Setup(uep => uep.GetNewValue(ExtColProperty.Min,  It.IsAny<DataColumn>())).Returns(1);
            updaterExcelProp.Setup(uep => uep.GetNewValue(ExtColProperty.Max,  It.IsAny<DataColumn>())).Returns(2);
            return updaterExcelProp;
        }

        /// UpdateIncludedPointsBasedFlawRange(double aboveX, double belowX, List<FixPoint> fixPoints)
        private List<FixPoint> _fixPointList;
        private Mock<ISortPointListWrapper> _sortByXList;
        [Test]
        public void UpdateIncludedPointsBasedFlawRange_SortByXDoesntHavePoints_FixPointsNotAddedAndTableNotUpdated()
        {
            //Arrange
            SetupSortByX(false);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _sortByXList.Verify(sbx => sbx.BinarySearch(It.IsAny<SortPoint>()), Times.Never);
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Never);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Flag>()), Times.Never);
            Assert.That(_fixPointList.Count, Is.Zero);
        }
        [Test]
        public void UpdateIncludedPointsBasedFlawRange_SortByXHasPointsAndIndicesAreGreaterThan0_BinarySearchPerformedWithoutFlippingBits()
        {
            //Arrange
            SetupSortByX(true);
            // setting both binary searches to 0 will ensure that the branch cases for _prevAbove and _prevBelow are never called for this test
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp=>sp.XValue == 0.1))).Returns(0);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0))).Returns(0);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _flipBinaryControl.Verify(fbc => fbc.FlipBits(It.IsAny<int>()), Times.Never);
            AsertBinarySearchCallsAndNoUpdateTableOrPointsAdded();
            Assert.That(_fixPointList.Count, Is.Zero);
        }
        [Test]
        [TestCase(-1, 0, 1)]
        [TestCase(0, -1, 1)]
        [TestCase(-1, -1, 2)]
        public void UpdateIncludedPointsBasedFlawRange_xAboveIndexAndOrXBelowIndexIsLessThanZero_FlipBitsCalledAccordingly(int above, int below, int expectedFlipBitsCalls)
        {
            //Arrange
            SetupSortByX(true);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 0.1))).Returns(above);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0))).Returns(below);
            // ensures no other branches are entered during these tests
            _flipBinaryControl.Setup(fbc => fbc.FlipBits(It.IsAny<int>())).Returns(0);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _flipBinaryControl.Verify(fbc => fbc.FlipBits(It.IsAny<int>()), Times.Exactly(expectedFlipBitsCalls));
            AsertBinarySearchCallsAndNoUpdateTableOrPointsAdded();
            Assert.That(_fixPointList.Count, Is.Zero);
        }
        private void AsertBinarySearchCallsAndNoUpdateTableOrPointsAdded()
        {
            _sortByXList.Verify(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 0.1)), Times.Once);
            _sortByXList.Verify(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0)), Times.Once);
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Never);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Flag>()), Times.Never);
        }
        // For the next four tests, the _prevAbove and _prevBelow values are 0 to start. Thus, negative numbers are used to check the rest of the branch cases
        [Test]
        public void UpdateIncludedPointsBasedFlawRange_xAboveIndexGreaterThanPreviousAbove_UpdateTablesCalledButNotGetCount()
        {
            //Arrange
            SetupSortByX(true);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 0.1))).Returns(1);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0))).Returns(0);
            _sortByXList.SetupGet(sbx => sbx.SortPointList).Returns(new List<SortPoint>() { new SortPoint() { SeriesPtIndex = 0 } });
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Never);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.InBounds), Times.Once);
            Assert.That(_fixPointList.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        [TestCase(3, 2)]
        [TestCase(2, 3)]
        [TestCase(1, 4)]
        public void UpdateIncludedPointsBasedFlawRange_xAboveIndexLessThanPreviousAbove_UpdateTablesCalledAndGetCount(int getCountNum, int expectedGetCountCalls)
        {
            //Arrange
            SetupSortByX(true);
            _data.PrevAbove = 2;
            SetupBinarySearchFunctionsAndList(xAboveIndex: 1, xBelowIndex: 0, listCount: getCountNum);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Exactly(expectedGetCountCalls));
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.OutBounds), Times.AtLeastOnce);
            Assert.That(_fixPointList.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        [TestCase(3, 2)]
        [TestCase(2, 3)]
        [TestCase(1, 4)]
        public void UpdateIncludedPointsBasedFlawRange_xBelowIndexLessThanPreviousBelow_UpdateTablesCalledAndGetCount(int getCountNum, int expectedGetCountCalls)
        {
            //Arrange
            SetupSortByX(true);
            _data.PrevBelow = 2;
            SetupBinarySearchFunctionsAndList(xAboveIndex: 0, xBelowIndex: 1, listCount: getCountNum);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Exactly(expectedGetCountCalls));
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.InBounds), Times.AtLeastOnce);
            Assert.That(_fixPointList.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        public void UpdateIncludedPointsBasedFlawRange_xBelowIndexGreaterThanPreviousBelow_UpdateTablesCalledButNotGetCount()
        {
            //Arrange
            SetupSortByX(true);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 0.1))).Returns(0);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0))).Returns(1);
            _sortByXList.SetupGet(sbx => sbx.SortPointList).Returns(new List<SortPoint>() { new SortPoint() { SeriesPtIndex = 0 } });
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Never);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.OutBounds), Times.Once);
            Assert.That(_fixPointList.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        [TestCase(0, false, 3)]
        [TestCase(0, true, 2)]
        [TestCase(3, false, 2)]
        [TestCase(3, true, 1)]
        public void UpdateIncludedPointsBasedFlawRange_xBelowIndexLessThanPreviousBelowAndPreviousBelowIsEqualToXAboeIndex_PreviousBelowDecreases(int xAboveIndex, bool prevBelowDoesNotInclude, int expectedIterations)
        {
            //Arrange
            SetupSortByX(true);
            //PrevAbove is set to xAboveIndex so that no additional fixpoints are added
            _data.PrevAbove = xAboveIndex;
            _data.PrevBelow = 3;
            _data.PrevBelowNotInclude = prevBelowDoesNotInclude;
            SetupBinarySearchFunctionsAndList(xAboveIndex: xAboveIndex, xBelowIndex: 1, listCount: 5);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.InBounds), Times.Exactly(expectedIterations));
            Assert.That(_fixPointList.Count, Is.GreaterThanOrEqualTo(1));
        }
        [Test]
        public void UpdateIncludedPointsBasedFlawRange_iBecomesNegative_ForLoopOnlyAddsWhileIIsPositive()
        {
            //Arrange
            SetupSortByX(true);
            //PrevAbove is set to xAboveIndex so that no additional fixpoints are added
            _data.PrevAbove = 2;
            _data.PrevBelow = 2;
            _data.PrevBelowNotInclude = true;
            SetupBinarySearchFunctionsAndList(xAboveIndex: 2, xBelowIndex: 1, listCount: 0);
            //Act
            _data.UpdateIncludedPointsBasedFlawRange(0.1, 1.0, _fixPointList);
            //Assert
            //For loop iterates once, but the AddFixPointsAndUpdateTable function is never called
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), Flag.InBounds), Times.Never);
            Assert.That(_fixPointList.Count, Is.Zero);
        }
        private void SetupBinarySearchFunctionsAndList(int xAboveIndex, int xBelowIndex, int listCount)
        {
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 0.1))).Returns(xAboveIndex);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.Is<SortPoint>(sp => sp.XValue == 1.0))).Returns(xBelowIndex);
            _sortByXList.Setup(sbx => sbx.GetCountOfList()).Returns(listCount);
            _sortByXList.SetupGet(sbx => sbx.SortPointList).Returns(new List<SortPoint>() { new SortPoint(), new SortPoint(), new SortPoint(), new SortPoint(), new SortPoint() });
        }
        private void  SetupSortByX(bool hasPoints)
        {
            _sortByXList = new Mock<ISortPointListWrapper>();
            _data.SortByXIn = _sortByXList.Object;
            _sortByXList.Setup(sbx => sbx.HasAnyPoints()).Returns(hasPoints);
            _fixPointList = new List<FixPoint>();
        }

        /// Tests for the  ToggleResponse(double pointX, double pointY, string seriesName, int rowIndex, int colIndex, List<FixPoint> fixPoints) function
        [Test]
        public void ToggleResponse_IndexIsNegativeFlippedIndexBitsGreaterThanSortByXCount_SortPointListAndUpdateTableNeverCalled()
        {
            //arrange
            SetupSortByXForToggleResponse(-1, 0);
            _flipBinaryControl.Setup(fbc => fbc.FlipBits(It.IsAny<int>())).Returns(1);
            //Act
            _data.ToggleResponse(1.0, 10.0, "", 1, 1, _fixPointList);
            //Assert
            _flipBinaryControl.Verify(fbc => fbc.FlipBits(It.IsAny<int>()), Times.Once);
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Once);
            _sortByXList.VerifyGet(sbx => sbx.SortPointList, Times.Never);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Flag>()), Times.Never);
        }
        [Test]
        [TestCase(2, 2, Flag.InBounds)]
        [TestCase(0, 0, Flag.OutBounds)]
        public void ToggleResponse_IndexIsNegativeButFlippedIndexBitsNOTGreaterThanSortByXCount_FoundPointAssignedWithUpdateTableAndFixPointsAddCalledAccordingly(
            int rowIndex, int colIndex, Flag expectedFlag)
        {
            //arrange
            _data.TurnOffPoint(rowIndex, colIndex);
            SetupSortByXForToggleResponse(-1, 1);
            _flipBinaryControl.Setup(fbc => fbc.FlipBits(It.IsAny<int>())).Returns(1);
            //Act
            _data.ToggleResponse(1.0, 10.0, "", 1, 1, _fixPointList);
            //Assert
            _flipBinaryControl.Verify(fbc => fbc.FlipBits(It.IsAny<int>()), Times.Once);
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Once);
            _sortByXList.VerifyGet(sbx => sbx.SortPointList, Times.Once);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), expectedFlag), Times.Once);
            Assert.That(_fixPointList.Count, Is.EqualTo(1));
        }
        [Test]
        [TestCase(1,1, Flag.InBounds)]
        [TestCase(0, 0, Flag.OutBounds)]
        public void ToggleResponse_IndexIsNotNegative_FoundPointAssignedWithUpdateTableAndFixPointsAddCalledAccordingly(int rowIndex, int colIndex, Flag expectedFlag)
        {
            //arrange            
            _data.TurnOffPoint(rowIndex, colIndex);
            SetupSortByXForToggleResponse(0, 0);
            //Act
            _data.ToggleResponse(1.0, 10.0, "", 1, 1, _fixPointList);
            //Assert
            _flipBinaryControl.Verify(fbc => fbc.FlipBits(It.IsAny<int>()), Times.Never);
            _sortByXList.Verify(sbx => sbx.GetCountOfList(), Times.Never);
            _sortByXList.VerifyGet(sbx => sbx.SortPointList, Times.Once);
            _updateTables.Verify(ut => ut.UpdateTable(It.IsAny<int>(), It.IsAny<int>(), expectedFlag), Times.Once);
            Assert.That(_fixPointList.Count, Is.EqualTo(1));
        }
        private void SetupSortByXForToggleResponse(int binarySearchVal, int countOfList)
        {
            SetupSortByX(true);
            _sortByXList.Setup(sbx => sbx.BinarySearch(It.IsAny<SortPoint>())).Returns(binarySearchVal);
            _sortByXList.Setup(sbx => sbx.SortPointList).Returns(new List<SortPoint>() { new SortPoint() { ColIndex = 1, RowIndex = 1 },
            new SortPoint() { ColIndex = 2, RowIndex = 2 } });
            _sortByXList.Setup(sbx => sbx.GetCountOfList()).Returns(countOfList);
        }
        /// Tests for the ForceRefillSortListAndClearPoints()
        [Test]
        public void ForceRefillSortListAndClearPoints_SortByXIsNull_CreatesTheListAndResetsAllTheSortListPoints()
        {
            //Arrange
            _data.TurnOffPoint(1, 1);
            _data.sortByX = null;
            //Act
            _data.ForceRefillSortListAndClearPoints();
            //Assert
            Assert.That(_data.TurnedOffPoints.Count, Is.Zero);
            Assert.That(_data.sortByX, Is.Not.Null);
            Assert.That(_data.sortByX.Count, Is.Zero);
        }
        [Test]
        public void ForceRefillSortListAndClearPoints_SortByXIsNOTNull_ResetsAllTheSortListPoints()
        {
            //Arrange
            _data.TurnOffPoint(1, 1);
            _data.sortByX = new List<SortPoint>() { new SortPoint() };
            //Act
            _data.ForceRefillSortListAndClearPoints();
            //Assert
            Assert.That(_data.TurnedOffPoints.Count, Is.Zero);
            Assert.That(_data.sortByX.Count, Is.Zero);
        }
        /// Tests for ForceRefillSortList function
        [Test]
        public void ForceRefillSortList_SortByXIsNull_CreatesTheListAndDoesNotResetTurnedOffPoints()
        {
            //Arrange
            _data.TurnOffPoint(1, 1);
            _data.sortByX = null;
            //Act
            _data.ForceRefillSortList();
            //Assert
            Assert.That(_data.TurnedOffPoints.Count, Is.GreaterThan(0));
            Assert.That(_data.sortByX, Is.Not.Null);
            Assert.That(_data.sortByX.Count, Is.Zero);
        }
        [Test]
        public void ForceRefillSortList_SortByXIsNOTNull_DoesNotResetTurnedOffPoints()
        {
            //Arrange
            _data.TurnOffPoint(1, 1);
            _data.sortByX = new List<SortPoint>() { new SortPoint() };
            //Act
            _data.ForceRefillSortList();
            //Assert
            Assert.That(_data.TurnedOffPoints.Count, Is.GreaterThan(0));
            Assert.That(_data.sortByX.Count, Is.Zero);
        }
        /// Tests for RefillSortList function
        [Test]
        public void RefillSortList_sortByXIsNull_CreatesListButFillListsNotCalled()
        {
            //Arrange
            SetupSortByX(true);
            _data.sortByX = null;
            SetupActivatedFlawsAndResponses();
            //Act
            _data.RefillSortList();
            //Assert
            Assert.That(_data.sortByX, Is.Not.Null);
            Assert.That(_data.sortByX.Count, Is.Zero);
        }
        [Test]
        public void RefillSortList_sortByXIsNullAndActivatedFlawsResponsesNotEmpty_CreatesListAndFillListsCalled()
        {
            //Arrange
            SetupSortByX(false);
            _data.sortByX = null;
            SetupActivatedFlawsAndResponses();
            //Act
            _data.RefillSortList();
            //Assert
            Assert.That(_data.sortByX, Is.Not.Null);
            Assert.That(_data.sortByX.Count, Is.GreaterThan(0));
        }
        [Test]
        public void RefillSortList_sortByXIsNOTNullAndActivatedFlawsResponsesNotEmpty_FillListsCalledAndAddedToSortByX()
        {
            //Arrange
            SetupSortByX(false);
            _data.sortByX = new List<SortPoint>() { new SortPoint() };
            SetupActivatedFlawsAndResponses();
            //Act
            _data.RefillSortList();
            //Assert
            Assert.That(_data.sortByX.Count, Is.GreaterThan(1));
        }
        private void SetupActivatedFlawsAndResponses()
        {
            _data.ActivatedFlaws.Columns.Add(new DataColumn("Flaws"));
            _data.ActivatedResponses.Columns.Add(new DataColumn("Responses"));
            for (int i =0; i < 11; i++)
            {
                _data.ActivatedFlaws.Rows.Add(Convert.ToDouble(i));
                _data.ActivatedResponses.Rows.Add(Convert.ToDouble(i * 10));
            }
        }
        /// ToggleAllResponses UNIT TESTS
        [Test]
        public void ToggleAllResponses_NoPointsFound_NoReponsesChanged()
        {
            //Arrange
            SetupActivatedFlawsAndResponses();
            SetupSortByX(true);
            _sortByXList.SetupGet(sbx => sbx.SortPointList).Returns(new List<SortPoint>());
            //Act
            _data.ToggleAllResponses(1.0, _fixPointList);
            //Assert
            _sortByXList.VerifyGet(sbx => sbx.SortPointList, Times.Once);
            Assert.That(_fixPointList.Count, Is.Zero);
            Assert.That(_data.TurnedOffPoints.Count, Is.Zero);
        }
        [Test]
        public void ToggleAllResponses_PointsFoundAndNotInTurnedOffPoints_FixPointListHasValueAndTurnedOffPointsContainsValue()
        {
            //Arrange
            SetupDataSourceToToggleAllResponses();
            //Act
            _data.ToggleAllResponses(10.0, _fixPointList);
            //Assert
            _sortByXList.VerifyGet(sbx => sbx.SortPointList, Times.Once);
            Assert.That(_fixPointList.Count, Is.EqualTo(1));
            Assert.That(_fixPointList[0].Flag, Is.EqualTo(Flag.OutBounds));
            Assert.That(_data.TurnedOffPoints.Count, Is.EqualTo(1));
        }
        [Test]
        public void ToggleAllResponses_PointsFoundAndInTurnedOffPoints_FixPointListHaveValueAndPointTurnedOn()
        {
            //Arrange
            SetupDataSourceToToggleAllResponses();
            //_data.TurnedOffPoints.Add(new DataPointIndex(1,1,""));
            _data.TurnOffPoint(0, 1);
            //Act
            _data.ToggleAllResponses(10, _fixPointList);
            //Assert
            Assert.That(_fixPointList.Count, Is.EqualTo(1));
            Assert.That(_fixPointList[0].Flag, Is.EqualTo(Flag.InBounds));
            Assert.That(_data.TurnedOffPoints.Count, Is.Zero);
        }
        private void SetupDataSourceToToggleAllResponses()
        {
            SetupSortByX(true);
            _sortByXList.SetupGet(sbx => sbx.SortPointList).Returns(new List<SortPoint>() { new SortPoint() { RowIndex = 1, ColIndex = 1, XValue = 10.0 } });
            var ahatTable = CreateSampleDataTable();
            for (int i = 0; i < 10; i++)
                ahatTable.Rows.Add(i, i * .25, i * 10);
            SetUpFakeData(out List<string> myFlaws, out List<string> myMetaDatas, out List<string> myResponses, out List<string> mySpecIDs);
            DataSource sourceWithActualData = SetupSampleDataSource(ahatTable);
            _data.SetSource(sourceWithActualData, myFlaws, myMetaDatas, myResponses, mySpecIDs);
        }
        /// Test for the AddData(string myID, double myFlaw, double myResponse, int index, IAddRowToTableControl addRowControlIn = null) function
        [Test]
        public void AddData_ValidArgsPassed_CallsStringRowToTableOnceAndDoubleRowToTableTwice()
        {
            //Arrange
            Mock<IAddRowToTableControl> addRowControl = new Mock<IAddRowToTableControl>();
            //Act
            _data.AddData("1", .1, 10.0, 1, addRowControl.Object);
            //Assert
            addRowControl.Verify(arc => arc.AddStringRowToTable("1", 1, It.IsAny<DataTable>()));
            addRowControl.Verify(arc => arc.AddDoubleRowToTable(.1, 1, It.IsAny<DataTable>()));
            addRowControl.Verify(arc => arc.AddDoubleRowToTable(10.0, 1, It.IsAny<DataTable>()));
        }
        /// Tests for the RecheckAnalysisType(AnalysisDataTypeEnum myForcedType) function
        [Test]
        [TestCase(AnalysisDataTypeEnum.AHat)]
        [TestCase(AnalysisDataTypeEnum.HitMiss)]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void RecheckAnalysisType_ActivatedResponsesCount0_DataTypeBecomesMyForcedTypeAndReturns(AnalysisDataTypeEnum myForcedType)
        {
            //Arrange
            //Act
            var result =_data.RecheckAnalysisType(myForcedType);
            //Assert
            Assert.That(_data.DataType, Is.EqualTo(myForcedType));
            Assert.That(result, Is.EqualTo(myForcedType));
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.AHat)]
        [TestCase(AnalysisDataTypeEnum.HitMiss)]
        public void RecheckAnalysisType_ActivatedResponsesNot0AndMyForcedTypeAHatOrHitMiss_DataTypeBecomesMyForcedTypeAndReturns(AnalysisDataTypeEnum myForcedType)
        {
            //Arrange
            SetupActivatedFlawsAndResponses();
            //Act
            var result = _data.RecheckAnalysisType(myForcedType);
            //Assert
            Assert.That(_data.DataType, Is.EqualTo(myForcedType));
            Assert.That(result, Is.EqualTo(myForcedType));
        }
        [Test]
        [TestCase(AnalysisDataTypeEnum.None)]
        [TestCase(AnalysisDataTypeEnum.Undefined)]
        public void RecheckAnalysisType_ActivatedResponsesNot0AndMyForcedTypeNotAHatOrHitMiss_AssignsDataTypeAsAHatAndReturnsBasedOnSampleActivatedResponses(AnalysisDataTypeEnum myForcedType)
        {
            //Arrange
            SetupActivatedFlawsAndResponses();
            //Act
            var result = _data.RecheckAnalysisType(myForcedType);
            //Assert
            Assert.That(_data.DataType, Is.EqualTo(AnalysisDataTypeEnum.AHat));
            Assert.That(result, Is.EqualTo(AnalysisDataTypeEnum.AHat));
        }
        /// Tests for the GetRow(int rowIndex, out string myID, out double myFlaw, out double myResponse) function
        [Test]
        [TestCase(9)]
        [TestCase(10)]
        public void GetRow_RowIndexGreaterThanOrEqualToAvailableSpecIDsTableRowsCount_ReturnsEmptyStringAnd0s(int testIndex)
        {
            //Arrange
            SetupDataSourceToToggleAllResponses(); //row count is 9
            //Act
            _data.GetRow(testIndex, out string myID, out double flaw, out double response);
            //Assert
            Assert.That(myID, Is.EqualTo(string.Empty));
            Assert.That(flaw, Is.Zero);
            Assert.That(response, Is.Zero);
        }
        [Test]
        public void GetRow_RowIndexLessThanAvailableSpecIDsTableRowsCount_AssignsParameters()
        {
            //Arrange
            SetupDataSourceToToggleAllResponses(); //row count is 9
            //Act
            _data.GetRow(8, out string myID, out double flaw, out double response);
            //Assert
            Assert.That(myID, Is.EqualTo("9"));
            Assert.That(flaw, Is.EqualTo(2.25));
            Assert.That(response, Is.EqualTo(90));
        }
        /// Test for the DeleteRow(int index) function
        [Test]
        public void DeleteRow_IndexPassed_RemovedTheRowFromSpecIDsFlawsAndResponses()
        {
            //Arrange
            SetupDataSourceToToggleAllResponses();//row count is 9
            //Act
            _data.DeleteRow(8);
            //Assert
            Assert.That(_data.RowCount, Is.EqualTo(8));
        }
        /// Test for EverythingCommented
        [Test]
        public void EverythingCommented_TurnedOffPointsIsEmpty_ReturnsTrue()
        {
            //Arrange
            //Act
            var result = _data.EverythingCommented;
            //Assert
            Assert.That(result, Is.True);
        }
        [Test]
        public void EverythingCommented_TurnedOffPointsIsNotEmptyAndCommentDictionaryNotNullAndTrimmedLengthIsNot0_ReturnsTrue()
        {
            //Arrange
            SetupDataSourceToToggleAllResponses();
            _data.TurnOffPoint(0, 1);
            _data.CommentDictionary[0][1] = "NotEmptyComment";
            //Act
            var result = _data.EverythingCommented;
            //Assert
            Assert.That(result, Is.True);
        }
        [Test]
        [TestCase(1, "")]
        [TestCase(1, " ")]
        [TestCase(1, "\t")]
        [TestCase(1, null)]
        [TestCase(0, "NotEmptyComment")]
        public void EverythingCommented_TurnedOffPointsIsNotEmptyAndEitherCommentDictionaryIsNullOrTrimmedLengthIs0_ReturnsFalse(int commentIndexDict, string comment)
        {
            //Arrange
            SetupDataSourceToToggleAllResponses();
            _data.TurnOffPoint(0, 1);
            _data.CommentDictionary[0][commentIndexDict] = comment;
            //Act
            var result = _data.EverythingCommented;
            //Assert
            Assert.That(result, Is.False);
        }


    }
}
