﻿//using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using NUnit.Framework;
using Moq;
using CSharpBackendWithR;
using POD.Analyze;
using POD;
using System.Data;
using POD.Data;
using POD.ExcelData;
using SpreadsheetLight;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Analyze.UnitTests
{
    [TestFixture]
    public class AnalysisTests
    {
        private Mock<IAnalysisData> _data;
        private Mock<ITemporaryLambdaCalc> _tempLambdaCalc;
        private Mock<IREngineObject> _rEngine;
        private Mock<I_IPy4C> _python;
        private Mock<IExcelExport> _excelOutput;
        private Mock<IAnalysisBackendControl> _analysisBackendControl;
        private Analysis _analysis;
        private DataTable _testDataTable;
        [SetUp]
        public void SetUp()
        {
            _data = new Mock<IAnalysisData>();
            _tempLambdaCalc = new Mock<ITemporaryLambdaCalc>();
            _rEngine = new Mock<IREngineObject>();
            _python = new Mock<I_IPy4C>();
            _excelOutput = new Mock<IExcelExport>();
            _analysisBackendControl = new Mock<IAnalysisBackendControl>();
            _analysis = new Analysis(_data.Object);
            _analysis.Name = "SampleAnalysis";
            _analysis.WorksheetName = "myWorkSheet";
        }
        private Analysis AnalysisWithNoDataMock()
        {
            var analysis = new Analysis();
            analysis.Name = "SampleAnalysis";
            analysis.WorksheetName = "myWorkSheet";
            return analysis;
        }
        private void SetPythonAndREngines()
        {
            _analysis.SetPythonEngine(_python.Object);
            _analysis.SetREngine(_rEngine.Object);
        }
        private void DataTableSampleSetupLinear(double scaler = 1.0)
        {
            _testDataTable = new DataTable();
            _testDataTable.Columns.Add("Test_Column_1");
            _testDataTable.Columns[0].DataType = typeof(double);
            for (int i =0; i< 11; i++)
            {
                _testDataTable.Rows.Add((double)i* scaler);
            }
        }
        /// <summary>
        /// Tests for the SetUpLambda() function
        /// </summary>
        [Test]
        public void SetUpLambda_ValidLambdaCalculated_AssignedLambdaToInLambdaValueField()
        {
            //Arrange
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            _data.SetupGet(ahat => ahat.AHATAnalysisObject).Returns(new AHatAnalysisObject("SampleAnalysis"));
            _python.Setup(ahat => ahat.AHatAnalysis("SampleAnalysis")).Returns(new AHatAnalysisObject("SampleAnalysis"));
            SetPythonAndREngines();
            _tempLambdaCalc.Setup(l => l.CalcTempLambda()).Returns(1.0);           
            //Act
            _analysis.SetUpLambda(_tempLambdaCalc.Object);
            //Assert
            Assert.That(_analysis.InLambdaValue, Is.EqualTo(1.0));
            Assert.That(_analysis.Data.AHATAnalysisObject.Lambda, Is.EqualTo(1.0));
        }

        /// <summary>
        /// Tests UpdateProgress(Object sender, int myProgressOutOF100) check if needed first
        /// </summary>

        /// <summary>
        /// Tests UpdateStatus(Object sender, string myCurrentStatus) check if needed first
        /// </summary>

        /// <summary>
        /// Tests AddError(Object sender, string myNewError) check if needed first
        /// </summary>

        /// <summary>
        /// Tests ClearErrors(Object sender) check if needed first
        /// </summary>

        /// <summary>
        /// tests for the ForceUpdateInputsFromData(bool recheckAnalysisType = false, AnalysisDataTypeEnum forcedType = AnalysisDataTypeEnum.Undefined) function
        /// </summary>
        [Test]
        public void ForceUpdateInputsFromData_UserSuppliedRanges_FlawAndResponseRangesRemainedUnchangedFromData()
        {
            //Arrange
            _analysis.UserSuppliedRanges = true;
            //Act
            _analysis.ForceUpdateInputsFromData();
            //Assert
            _data.Verify(data => data.InvertTransformedResponse(It.IsAny<double>()), Times.Never);
            _data.Verify(data => data.InvertTransformedFlaw(It.IsAny<double>()), Times.Never);
            _data.Verify(data => data.RecheckAnalysisType(It.IsAny<AnalysisDataTypeEnum>()), Times.Never);
            _data.VerifyGet(data => data.DataType, Times.Once);
        }
        [Test]
        public void ForceUpdateInputsFromData_UserDidNotSuppliedRanges_FlawAndResponseRangesRemainedUnchangedFromData()
        {
            //Arrange
            FlawResponseRangeSetup();
            DataTableSampleSetupLinear();
            DataInvertTransformSetup();
            //The response range is set by  min + ((max - min) * .15 (%15 of the total response range)
            _data.Setup(data => data.InvertTransformedResponse(-1.0 +(11.0-(-1.0))*.15)).Returns(Math.Exp(-1.0 + (11.0 - (-1.0)) * .15));
            //Act
            _analysis.ForceUpdateInputsFromData();
            //Assert 
            _data.Verify(data => data.InvertTransformedResponse(It.IsAny<double>()), Times.AtLeastOnce);
            _data.Verify(data => data.InvertTransformedFlaw(It.IsAny<double>()), Times.AtLeastOnce);
            _data.Verify(data => data.RecheckAnalysisType(It.IsAny<AnalysisDataTypeEnum>()), Times.Never);
            _data.VerifyGet(data => data.DataType, Times.Once);
            Assert.That(_analysis.InFlawMax, Is.Not.EqualTo(1.0));
            Assert.That(_analysis.InResponseMax, Is.Not.EqualTo(100.0));
        }
        [Test]
        public void ForceUpdateInputsFromData_RecheckAnalysisTypeTrue_AnalysisDataTypeOverwritten()
        {
            //Arrange
            _analysis.UserSuppliedRanges = true;
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            _data.Setup(data => data.RecheckAnalysisType(AnalysisDataTypeEnum.HitMiss)).Returns(AnalysisDataTypeEnum.HitMiss);
            //Act
            _analysis.ForceUpdateInputsFromData(true, AnalysisDataTypeEnum.HitMiss);
            //Assert 
            Assert.That(_analysis.AnalysisDataType, Is.EqualTo(AnalysisDataTypeEnum.HitMiss));
        }
        private void FlawResponseRangeSetup()
        {
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _analysis.InFlawMin = .10;
            _analysis.InFlawMax = 1.0;
            _analysis.InResponseMin = 10.0;
            _analysis.InResponseMax = 100.0;
        }
        private void DataInvertTransformSetup()
        {
            _data.SetupGet(data => data.ActivatedFlaws).Returns(_testDataTable);
            _data.SetupGet(data => data.ActivatedResponses).Returns(_testDataTable);
            _data.Setup(data => data.InvertTransformedResponse(-1)).Returns(Math.Exp(0));
            _data.Setup(data => data.InvertTransformedResponse(11)).Returns(Math.Exp(11));
            _data.Setup(data => data.InvertTransformedFlaw(-1)).Returns(Math.Exp(0));
            _data.Setup(data => data.InvertTransformedFlaw(11)).Returns(Math.Exp(11));
        }
        /// <summary>
        /// Tests for the CalculateInitialValuesWithNewData() function
        /// </summary>
        [Test]
        public void CalculateInitialValuesWithNewData_PythonIsNullAndHasBeenInitizlizedFalse_HasBeenInitializedStillFalse()
        {
            //Arrange
            Analysis analysis = new Analysis(_data.Object);
            analysis.HasBeenInitialized = false;
            analysis.SetPythonEngine(null);
            //Act
            analysis.CalculateInitialValuesWithNewData();
            //Assert
            _data.Verify(sp=>sp.SetPythonEngine((I_IPy4C)null, (String)null), Times.Once);
            Assert.That(analysis.Python, Is.Null);
            Assert.That(analysis.HasBeenInitialized, Is.False);
        }
        [Test]
        public void CalculateInitialValuesWithNewData_PythonIsNullAndHasBeenInitizlizedTrue_HasBeenInitializedIsStillTrueAndPythonNull()
        {
            //Arrange
            Analysis analysis = new Analysis(_data.Object);
            analysis.HasBeenInitialized = true;
            analysis.SetPythonEngine(null);
            //Act
            analysis.CalculateInitialValuesWithNewData();
            //Assert
            _data.Verify(sp => sp.SetPythonEngine((I_IPy4C)null, (String)null), Times.Once);
            Assert.That(analysis.HasBeenInitialized, Is.True);
            Assert.That(analysis.Python, Is.Null);
        }
        [Test]
        public void CalculateInitialValuesWithNewData_PythonIsNotNullAndHasBeenInitizlizedTrue_HasBeenInitializedIsStillTrueAndPythonNotNull()
        {
            //Arrange           
            _analysis.HasBeenInitialized = true;
            SetPythonAndREngines();
            //Act
            _analysis.CalculateInitialValuesWithNewData();
            //Assert
            _data.Verify(sp => sp.SetPythonEngine(_python.Object, "SampleAnalysis"), Times.Once);
            Assert.That(_analysis.HasBeenInitialized, Is.True);
            Assert.That(_analysis.Python, Is.Not.Null);
        }
        [Test]
        public void CalculateInitialValuesWithNewData_PythonNotNullAndHasBeenInitizlizedFalseNotHitMiss_ForceUpdateFunctionFired()
        {
            //Arrange           
            _analysis.HasBeenInitialized = false;
            AssignActivatedFlawsAndResponses();
            SetPythonAndREngines();
            //Act
            _analysis.CalculateInitialValuesWithNewData();
            //Assert
            Assert.That(_analysis.HasBeenInitialized, Is.True);
            _python.Verify(te => te.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Never);
            // These values are set when ForceUpdateFunctionIsFired
            AssertForceUpdateFunctionIsFired();

            Assert.That(_analysis.AnalysisDataType, Is.EqualTo(AnalysisDataTypeEnum.AHat));
            Assert.That(_analysis.InFlawTransform, Is.EqualTo(TransformTypeEnum.Linear));
        }
        [Test]
        public void CalculateInitialValuesWithNewData_PythonNotNullAndHasBeenInitizlizedFalseIsHitMiss_ForceUpdateFunctionFiredAndHMModelSetToLog()
        {
            //Arrange
            //Analysis analysis=AnalysisWithNoDataMock();
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.HitMiss;
            _analysis.HasBeenInitialized = false;
            AssignActivatedFlawsAndResponses();
            _data.SetupGet(hitmiss => hitmiss.HMAnalysisObject).Returns(new HMAnalysisObject("SampleAnalysis"));
            _data.SetupGet(dt => dt.DataType).Returns(AnalysisDataTypeEnum.HitMiss);
            _python.Setup(p => p.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _python.Setup(hitmiss => hitmiss.HitMissAnalsysis("SampleAnalysis")).Returns(new HMAnalysisObject("SampleAnalysis"));
            SetPythonAndREngines();
            //Act
            _analysis.CalculateInitialValuesWithNewData();
            //Assert
            Assert.That(_analysis.HasBeenInitialized, Is.True);
            _python.Verify(te => te.TransformEnumToInt(It.IsAny<TransformTypeEnum>()), Times.Once);
            AssertForceUpdateFunctionIsFired();
            Assert.That(_analysis.AnalysisDataType, Is.EqualTo(AnalysisDataTypeEnum.HitMiss));
            Assert.That(_analysis.InFlawTransform, Is.EqualTo(TransformTypeEnum.Log));
            Assert.That(_analysis.Data.FlawTransform, Is.EqualTo(TransformTypeEnum.Log));
        }
        // These values are set when ForceUpdateFunctionIsFired

        private void AssertForceUpdateFunctionIsFired()
        {
            _data.VerifyGet(data => data.DataType, Times.Once);
        }
        private void AssignActivatedFlawsAndResponses()
        {
            _data.SetupGet(ar => ar.ActivatedResponses).Returns(new DataTable());
            _data.SetupGet(ar => ar.ActivatedFlaws).Returns(new DataTable());
        }
        /// <summary>
        /// Tests for the GetBufferedMinMax(DataTable myTable, out double myMin, out double myMax) function
        /// Note: this function does not accept negative flaw values
        /// Another Note: These tests are a bit specific, if the min and max buffered changes, Simply assert that myMin and myMax are valid doubles (i.e. not Double.NaN)
        /// </summary>
        [Test]
        public void GetBufferedMinMax_EmptyDataTable_ReturnsNegativePt1MinAnd1Pt1Max()
        {
            //Arrange
            DataTable emptyDataTable = new DataTable();
            double myMin = Double.NaN;
            double myMax = Double.NaN;
            //Act
            Analysis.GetBufferedMinMax(emptyDataTable, out myMin, out myMax);
            //Assert
            Assert.That(myMin, Is.EqualTo(-.1));
            Assert.That(myMax, Is.EqualTo(1.1));

        }
        [Test]
        public void GetBufferedMinMax_NonEmptyDataTableLinear_ReturnsBufferedMinAndMaxOfDataTable()
        {
            //Arrange
            DataTableSampleSetupLinear();
            double myMin = Double.NaN;
            double myMax = Double.NaN;
            //Act
            Analysis.GetBufferedMinMax(_testDataTable, out myMin, out myMax);
            //Assert
            Assert.That(myMin, Is.EqualTo(-1));
            Assert.That(myMax, Is.EqualTo(11));

        }

        /// <summary>
        /// Tests for the CreateDuplicate() function
        /// </summary>
        [Test]
        public void CreateDuplicate_AnalysisObjectInitialized_ReturnsClonedAnalysisObject()
        {
            //Arrange
            DataSource source = new DataSource("DataSource", "ID", "Flaw", "Response");
            //_data.Setup(cd => cd.CreateDuplicate()).Returns(_data.Object);
            _analysis.SetDataSource(source);
            var placeholder=_analysis.Data.CommentDictionary;
            SetPythonAndREngines();
            //Act
            Analysis clone = _analysis.CreateDuplicate();
            //Assert
            Assert.That(_analysis != clone);
            Assert.That(clone.Python, Is.Null);
            Assert.That(_analysis.Python, Is.Not.Null);
            _data.Verify(data => data.CreateDuplicate(), Times.Once);
        }

        /// <summary>
        /// Tests for the RunAnalysis(bool quickAnalysis=false) function
        /// </summary>
        [Test]
        [Ignore("Need to figure out how to mock a background worker")]
        public void RunAnalysis_NotQuickAnalysis_ReturnsTrue()
        {
        }
        // <summary>
        /// Tests for the RunAnalysis(bool quickAnalysis=false) function
        /// </summary>
        [Test]
        public void UpdateRTransforms_AnalysisTypeHitMissNoChange_ModelHitMissTheSame()
        {
            var analysis =SetupIPy4CTransformsHitMiss(TransformTypeEnum.Linear, 1);
            
            var originalModel = analysis.Data.HMAnalysisObject.ModelType;
            //don't change the flaw transform to log
            //Act
            analysis.UpdateRTransforms();
            //Assert
            Assert.That(analysis.Data.HMAnalysisObject.ModelType, Is.EqualTo(originalModel));
        }

        // <summary>
        /// Tests for the UpdateRTransforms() function
        /// </summary>
        [Test]
        [TestCase(TransformTypeEnum.Log , 2)]
        [TestCase(TransformTypeEnum.Inverse, 3)]
        public void UpdateRTransforms_AnalysisTypeHitMissChangesTransform_ReturnsModelUpdateForHitMiss(TransformTypeEnum transformChange, int expectedModelType)
        {
            var analysis = SetupIPy4CTransformsHitMiss(transformChange, expectedModelType);
            //change the flaw transform to log
            analysis.InFlawTransform = transformChange;
            //Act
            analysis.UpdateRTransforms();
            //Assert
            Assert.That(analysis.Data.HMAnalysisObject.ModelType, Is.EqualTo(expectedModelType));
        }
        [Test]
        public void UpdateRTransforms_AnalysisTypeAHatNoChange_TransformsAndModelSame()
        {
            var analysis = SetupIPy4CTransformsAhat();
            SetPythonAndREngines();
            var originalModel = analysis.Data.AHATAnalysisObject.ModelType;
            //dont change transforms
            //Act
            analysis.UpdateRTransforms();
            //Assert
            Assert.That(analysis.Data.AHATAnalysisObject.ModelType, Is.EqualTo(originalModel));
        }
        [Test]
        [TestCase(TransformTypeEnum.Linear, TransformTypeEnum.Linear, 1)]
        [TestCase(TransformTypeEnum.Log, TransformTypeEnum.Linear, 2)]
        [TestCase(TransformTypeEnum.Linear, TransformTypeEnum.Log, 3)]
        [TestCase(TransformTypeEnum.Log, TransformTypeEnum.Log, 4)]
        [TestCase(TransformTypeEnum.Linear, TransformTypeEnum.BoxCox, 5)]
        [TestCase(TransformTypeEnum.Log, TransformTypeEnum.BoxCox, 6)]
        [TestCase(TransformTypeEnum.Inverse, TransformTypeEnum.BoxCox, 7)]
        [TestCase(TransformTypeEnum.Linear, TransformTypeEnum.Inverse, 8)]
        [TestCase(TransformTypeEnum.Log, TransformTypeEnum.Inverse, 9)]
        [TestCase(TransformTypeEnum.Inverse, TransformTypeEnum.Linear, 10)]
        [TestCase(TransformTypeEnum.Inverse, TransformTypeEnum.Log, 11)]
        [TestCase(TransformTypeEnum.Inverse, TransformTypeEnum.Inverse, 12)]

        public void UpdateRTransforms_AnalysisTypeAHatChanges_TransformsAndModelSame(TransformTypeEnum transformChangeX, TransformTypeEnum transformChangeY, int expectedModelType)
        {
            var analysis = SetupIPy4CTransformsAhat();
            SetPythonAndREngines();
            //change the flaw transform to log
            analysis.InFlawTransform = transformChangeX;
            analysis.InResponseTransform = transformChangeY;
            //Act
            analysis.UpdateRTransforms();
            //Assert
            Assert.That(analysis.Data.AHATAnalysisObject.ModelType, Is.EqualTo(expectedModelType));
        }
        private Analysis SetupIPy4CTransformsHitMiss(TransformTypeEnum testTransformX, int expectedOutput)
        {
            var analysis = AnalysisWithNoDataMock();
            analysis.AnalysisDataType = AnalysisDataTypeEnum.HitMiss;
            analysis.Data.DataType = AnalysisDataTypeEnum.HitMiss;
            _python.Setup(hitmiss => hitmiss.HitMissAnalsysis("SampleAnalysis")).Returns(new HMAnalysisObject("SampleAnalysis"));
            _python.Setup(modelType => modelType.TransformEnumToInt(testTransformX)).Returns(expectedOutput);
            analysis.SetPythonEngine(_python.Object);
            analysis.SetREngine(_rEngine.Object);
            return analysis;
        }
        private Analysis SetupIPy4CTransformsAhat()
        {
            var analysis = AnalysisWithNoDataMock();
            analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            analysis.Data.DataType = AnalysisDataTypeEnum.AHat;
            _python.Setup(ahat => ahat.AHatAnalysis("SampleAnalysis")).Returns(new AHatAnalysisObject("SampleAnalysis"));
            //setup all possible transforms since AHat could be either
            _python.Setup(modelType => modelType.TransformEnumToInt(TransformTypeEnum.Linear)).Returns(1);
            _python.Setup(modelType => modelType.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _python.Setup(modelType => modelType.TransformEnumToInt(TransformTypeEnum.Inverse)).Returns(3);
            _python.Setup(modelType => modelType.TransformEnumToInt(TransformTypeEnum.BoxCox)).Returns(5);
            analysis.SetPythonEngine(_python.Object);
            analysis.SetREngine(_rEngine.Object);
            return analysis;
        }
        /// <summary>
        /// Tests for the SetDataSource(DataSource mySource) function
        /// </summary>

        [Test]
        public void SetDataSource_ValidDataSourcePassed_DataSetSourceFunctionExecuted()
        {
            //Arrange
            DataSource source = new DataSource("MyDataSource", "ID", "flawName.centimeters", "Response");
            //Act
            _analysis.SetDataSource(source);
            //Assert
            _data.Verify(data => data.SetSource(source), Times.Once);
        }
        /// <summary>
        /// Tests for the SetDataSource(DataSource mySource, List<string> myFlaws, List<string> myMetaDatas,
        /// List<string> myResponses, List<string> mySpecIDs) function
        /// </summary>

        [Test]
        public void SetDataSourceMultipleStrings_ValidDataSourcePassed_DataSetSourceFunctionExecuted()
        {
            //Arrange
            DataSource source = new DataSource("MyDataSource", "ID", "flawName.centimeters", "Response");
            //Act
            _analysis.SetDataSource(source, new List<string>() { "flaws" }, new List<string>() { "metadata" },
                new List<string>() { "responses" }, new List<string>() { "specIDs" });
            //Assert
            _data.Verify(data => data.SetSource(source, It.IsAny<List<string>>(), It.IsAny<List<string>>(), It.IsAny<List<string>>(),
                It.IsAny<List<string>>()), Times.Once);
        }

        /// <summary>
        /// Tests for the public void WriteToExcel(ExcelExport myWriter, bool myPartOfProject = true, DataTable table = null) function
        /// </summary>

        /// TODO: write tests for this method
        /*
        [Test]
        public void WriteToExcel_PartOfProjectAndTableIsNull_WorSheetNameNotOverwrittenAndRemoveDefaultSheetNotCalled()
        {
            //Arrange
            SetUpExcelWriting();
            //Act
            _analysis.WriteToExcel(_excelOutput.Object, true);

            Assert.That(_analysis.WorksheetName, Is.EqualTo("myWorkSheet"));
            _excelOutput.Verify(e => e.RemoveDefaultSheet(), Times.Never);
        }
        */


        /// <summary>
        ///  WriteQuickAnalysis(ExcelExport myWriter, DataTable myInputTable, string myOperator, string mySpecimentSet, string mySpecUnits, double mySpecMin, double mySpecMax,
        ///  string myInstrument = "", string myInstUnits = "", double myInstMin = 0.0, double myInstMax = 1.0)
        /// </summary>
        [Test]
        public void WriteQuickAnalysis_WriteHitMissQuickAnalysis_WrittenToExcelWithNoInstrumentOrInstMinMax()
        {
            //Arrange
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.HitMiss;
            _analysis.Data.DataType = AnalysisDataTypeEnum.HitMiss;
            SetUpExcelWriting();
            //Act
            _analysis.WriteQuickAnalysis(_excelOutput.Object, new DataTable(), "operator", "specimentSet", "specUnits", 0.0, 10.0, "instrument", "units");
            //Assert
            VerifySetCells(Times.Never);
            /// This test will still pass in the event any cells are added or changed
            VerifySetCellsCount(13, 2);
        }
        [Test]
        public void WriteQuickAnalysis_WriteAHatQuickAnalysis_WrittenToExcelWithAnInstrumentAndInstMinMax()
        {
            //Arrange          
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            _analysis.Data.DataType = AnalysisDataTypeEnum.AHat;
            SetPythonAndREngines();
            var spreadSheet = new SLDocument();
            _excelOutput.SetupGet(w => w.Workbook).Returns(spreadSheet);
            //Act
            _analysis.WriteQuickAnalysis(_excelOutput.Object, new DataTable(), "operator", "specimentSet", "specUnits", 0.0, 10.0, "instrument", "units");
            //Assert
            VerifySetCells(Times.Once);
            /// This test will still pass in the event any cells are added or changed
            VerifySetCellsCount(18, 4);
        }
        private void SetUpExcelWriting()
        {
            SetPythonAndREngines();
            var spreadSheet = new SLDocument();
            _excelOutput.SetupGet(w => w.Workbook).Returns(spreadSheet);
        }
        private void VerifySetCells(Func<Times> shouldExecute)
        {
            _excelOutput.Verify(e => e.SetCellValue(1, 1, "Quick Analysis"), Times.Once);
            _excelOutput.Verify(e => e.SetCellValue(It.IsAny<int>(), 1, "Instrument"), shouldExecute);
            _excelOutput.Verify(e => e.SetCellValue(It.IsAny<int>(), 1, "RESPONSE:"), shouldExecute);
            _excelOutput.Verify(e => e.SetCellValue(It.IsAny<int>(), 2, "instrument"), shouldExecute);
            _excelOutput.Verify(e => e.RemoveDefaultSheet(), Times.Once);
        }
        private void VerifySetCellsCount(int countString, int countWithDouble)
        {
            _excelOutput.Verify(e => e.SetCellValue(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.AtLeast(countString));
            _excelOutput.Verify(e => e.SetCellValue(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()), Times.AtLeast(countWithDouble));
        }

        /// <summary>
        /// Test for the public double TransformValueForXAxis(double myValue) function
        /// ensure a double is always returned
        /// </summary>
        [Test]
        public void TransformValueForXAxis_AnyDoublePassed_ReturnsAValidDouble()
        {
            //Arrange
            SetPythonAndREngines();
            var myValue = It.IsAny<double>();

            //Act
            var result = _analysis.TransformValueForXAxis(myValue);

            //Assert
            Assert.That(result, Is.Not.EqualTo(double.NaN));
        }

        /// <summary>
        /// Test for the public decimal TransformValueForXAxis(decimal myValue) function
        /// </summary>
        //The possible transforms that can go into this function is Linear, Log, and inverse
        [Test]
        [TestCase(TransformTypeEnum.Linear, 1, 2.0, 2.0)]
        [TestCase(TransformTypeEnum.Linear, 1, 0.0, 0.0)]
        [TestCase(TransformTypeEnum.Linear, 1, -2.0, -2.0)]
        [TestCase(TransformTypeEnum.Inverse, 3, 2.0, .5)]
        [TestCase(TransformTypeEnum.Inverse, 3, -2.0, -.5)]
        public void TransformValueForXAxis_NonLogtransformPassed_ReturnsValidtransform(TransformTypeEnum transform, int enumTransform, decimal myValue,  decimal expectedResult)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = transform;
            _python.Setup(e => e.TransformEnumToInt(transform)).Returns(enumTransform);
            //Act
            var result = _analysis.TransformValueForXAxis(myValue);
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        [Test]
        public void TransformValueForXAxis_0PassedInForInverseTransform_ThrowsOverflowExcpetionAndReturnsTheSameValue()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Inverse;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Inverse)).Returns(3);
            //Act
            var result = _analysis.TransformValueForXAxis(0.0M);
            //Assert
            Assert.That(result, Is.EqualTo(0.0M));
        }
        [Test]
        [TestCase(Math.E, 1.0)]
        [TestCase(.1, -2.303)]
        public void TransformValueForXAxis_LogTransformPassedAndValueIsPositive_ReturnsValidLogTransform(decimal inputValue, decimal expectedValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            //Act
            var result = _analysis.TransformValueForXAxis(inputValue);
            //Assert
            Assert.That(Math.Round(result, 3), Is.EqualTo(expectedValue));

        }
        [Test]
        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void TransformValueForXAxis_LogTransformPassedAndValueIsNegativeOr0AndSmallestFlawIsNot0_ReturnsValidLogTransform(decimal inputValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.SetupGet(sf => sf.SmallestFlaw).Returns(.1);
            //Act
            var result = _analysis.TransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(Convert.ToDecimal(Math.Log(.1/2.0))));

        }
        [Test]
        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void TransformValueForXAxis_LogTransformPassedAndValueIsNegativeOr0AndSmallestFlawIs0_ThrowsExceptionAndReturns0(decimal inputValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.SetupGet(sf => sf.SmallestFlaw).Returns(0.0);
            //Act
            var result = _analysis.TransformValueForXAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(0.0M));
        }

        /// <summary>
        /// Test for the public double TransformValueForYAxis(double myValue) function
        /// ensure a double is always returned
        /// </summary>
        [Test]
        public void TransformValueForYAxis_AnyDoublePassed_ReturnsAValidDouble()
        {
            //Arrange
            SetPythonAndREngines();
            var myValue = It.IsAny<double>();
            //Act
            var result = _analysis.TransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.Not.EqualTo(double.NaN));
        }

        /// <summary>
        /// Test for the public decimal TransformValueForYAxis(decimal myValue) function
        /// </summary>
        //The possible transforms that can go into this function is Linear, Log, BoxCox, and inverse
        [Test]
        [TestCase(TransformTypeEnum.Linear, 1, 2.0, 2.0)]
        [TestCase(TransformTypeEnum.Linear, 1, 0.0, 0.0)]
        [TestCase(TransformTypeEnum.Linear, 1, -2.0, -2.0)]
        [TestCase(TransformTypeEnum.Inverse, 3, 2.0, .5)]
        [TestCase(TransformTypeEnum.Inverse, 3, -2.0, -.5)]
        public void TransformValueForYAxis_NonLogOrBoxCoxtransformPassed_ReturnsValidtransform(TransformTypeEnum transform, 
            int enumTransform, decimal myValue, decimal expectedResult)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = transform;
            _python.Setup(e => e.TransformEnumToInt(transform)).Returns(enumTransform);
            //Act
            var result = _analysis.TransformValueForYAxis(myValue);
            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        public void TransformValueForYAxis_0PassedInForInverseTransform_ThrowsOverflowExcpetionAndReturnsTheSameValue()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Inverse;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Inverse)).Returns(3);
            //Act
            var result = _analysis.TransformValueForYAxis(0.0M);
            //Assert
            Assert.That(result, Is.EqualTo(0.0M));
        }
        [Test]
        [TestCase(Math.E, 1.0)]
        [TestCase(.1, -2.303)]
        public void TransformValueForYAxis_LogTransformPassedAndValueIsPositive_ReturnsValidLogTransform(decimal inputValue, decimal expectedValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            //Act
            var result = _analysis.TransformValueForYAxis(inputValue);
            //Assert
            Assert.That(Math.Round(result, 3), Is.EqualTo(expectedValue));

        }
        [Test]
        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void TransformValueForYAxis_LogTransformPassedAndValueIsNegativeOr0AndSmallestFlawIsNot0_ReturnsValidLogTransform(decimal inputValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.SetupGet(sf => sf.SmallestResponse).Returns(.1);
            //Act
            var result = _analysis.TransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(Convert.ToDecimal(Math.Log(.1 / 2.0))));

        }
        [Test]
        [TestCase(0.0)]
        [TestCase(-1.0)]
        public void TransformValueForYAxis_LogTransformPassedAndValueIsNegativeOr0AndSmallestFlawIs0_ThrowsExceptionAndReturns0(decimal inputValue)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.SetupGet(sf => sf.SmallestResponse).Returns(0.0);
            //Act
            var result = _analysis.TransformValueForYAxis(inputValue);
            //Assert
            Assert.That(result, Is.EqualTo(0.0M));
        }
        //Testing for a postive and negative value for lambda
        [Test]
        [TestCase(.5, 16.0, 6.0)]
        [TestCase(-.5, 16.0, 1.5)]
        public void TransformValueForYAxis_BoxCoxTransformPassedAndMyValueIsPositive_ReturnsValidBoxCoxTransform(double lambdaValue, decimal myValue, decimal ExpectedOutput)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.BoxCox;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.BoxCox)).Returns(5);
            _data.SetupGet(sf => sf.AHATAnalysisObject).Returns(new AHatAnalysisObject("SampleAnalysis"));
            _analysis.InLambdaValue = lambdaValue;
            //Act
            var result = _analysis.TransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(ExpectedOutput));
        }
        [Test]
        [TestCase(.5, -16.0, -1.0)]
        [TestCase(-.5, -16.0, 0.0)]
        public void TransformValueForYAxis_BoxCoxTransformPassedAndMyValueIsNegative_ReturnsNegative1ForPositiveLambdaAndZeroForNegativeLambdas(double lambdaValue,
            decimal myValue, decimal ExpectedOutput)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.BoxCox;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.BoxCox)).Returns(5);
            _data.SetupGet(sf => sf.AHATAnalysisObject).Returns(new AHatAnalysisObject("SampleAnalysis"));
            _analysis.InLambdaValue = lambdaValue;
            //Act
            var result = _analysis.TransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(ExpectedOutput));
        }


        /// <summary>
        /// Test for the public double InvertTransformValueForXAxis(double myValue) function
        /// ensure a double is always returned
        /// </summary>
        [Test]
        public void InvertTransformValueForXAxis_AnyDoublePassed_ReturnsAValidDouble()
        {
            //Arrange
            SetPythonAndREngines();
            var myValue = It.IsAny<double>();

            //Act
            var result = _analysis.InvertTransformValueForXAxis(myValue);

            //Assert
            Assert.That(result, Is.Not.EqualTo(double.NaN));
        }

        /// <summary>
        /// Test for the public double TransformValueForYAxis(decimal myValue) function
        /// </summary>
        [Test]
        [TestCase(2, 0.0, 1.0)]
        public void InvertTransformValueForXAxis_ValidValuePassedAndPythonNotNull_ReturnsValidtransform(int enumTransform, decimal myValue, decimal expectedResult)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(enumTransform);
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), enumTransform)).Returns(Convert.ToDouble(expectedResult));
            //Act
            var result = _analysis.InvertTransformValueForXAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        [Test]
        [TestCase(-1.0)]
        [TestCase(0.0)]
        [TestCase(1.0)]
        public void InvertTransformValueForXAxis_ValidValuePassedPythonIsNull_ReturnsTheSameValue(decimal myValue)
        {
            //Arrange
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), 2)).Returns(Convert.ToDouble(2.0));
            //Act
            var result = _analysis.InvertTransformValueForXAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(myValue));
        }
        [Test]
        [TestCase(1.0)]
        public void InvertTransformValueForXAxis_ThrowsException_ReturnsTheSameValue(decimal myValue)
        {
            //Arrange
            SetupTransformBackFlaws();
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), 2)).Throws<Exception>();
            //Act
            var result = _analysis.InvertTransformValueForXAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(myValue));
        }
        private void SetupTransformBackFlaws()
        {
            SetPythonAndREngines();
            _analysis.InFlawTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
        }

        /// <summary>
        /// Test for the public decimal TransformValueForYAxis(decimal myValue) function
        /// </summary>
        [Test]
        [TestCase(2, 0.0, 1.0)]
        public void InvertTransformValueForYAxis_ValidValuePassedAndPythonNotNull_ReturnsValidtransform(int enumTransform, decimal myValue, decimal expectedResult)
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(enumTransform);
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), enumTransform)).Returns(Convert.ToDouble(expectedResult));
            //Act
            var result = _analysis.InvertTransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(expectedResult));
        }
        [Test]
        [TestCase(-1.0)]
        [TestCase(0.0)]
        [TestCase(1.0)]
        public void InvertTransformValueForYAxis_ValidValuePassedPythonIsNull_ReturnsTheSameValue(decimal myValue)
        {
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), 2)).Returns(Convert.ToDouble(2.0));
            //Act
            var result = _analysis.InvertTransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(myValue));
        }
        [Test]
        [TestCase(1.0)]
        public void InvertTransformValueForYAxis_ThrowsException_ReturnsTheSameValue(decimal myValue)
        {
            SetupTransformBackResponse();
            _data.Setup(transB => transB.TransformBackAValue(Convert.ToDouble(myValue), 2)).Throws<Exception>();
            //Act
            var result = _analysis.InvertTransformValueForYAxis(myValue);
            //Assert
            Assert.That(result, Is.EqualTo(myValue));
        }
        private void SetupTransformBackResponse()
        {
            SetPythonAndREngines();
            _analysis.InResponseTransform = TransformTypeEnum.Log;
            _python.Setup(e => e.TransformEnumToInt(TransformTypeEnum.Log)).Returns(2);
        }

        /// <summary>
        /// Test for the public void UpdateRangesFromData() function
        /// </summary>
        [Test]
        public void UpdateRangesFromData_HasBeenInitializedTrue_GetUpdatedValueFunctionFired()
        {
            //Arrange
            _analysis.HasBeenInitialized = true;
            //Act
            _analysis.UpdateRangesFromData();
            //Assert
            var tempValue = 0.0;
            _data.Verify(guv => guv.GetUpdatedValue(ColType.Flaw, ExtColProperty.Max, It.IsAny<double>(), out tempValue), Times.Once);
            _data.Verify(guv => guv.GetUpdatedValue(ColType.Flaw, ExtColProperty.Min, It.IsAny<double>(), out tempValue), Times.Once);
            _data.Verify(guv => guv.GetUpdatedValue(ColType.Response, ExtColProperty.Max, It.IsAny<double>(), out tempValue), Times.Once);
            _data.Verify(guv => guv.GetUpdatedValue(ColType.Response, ExtColProperty.Min, It.IsAny<double>(), out tempValue), Times.Once);
            _data.Verify(guv => guv.GetUpdatedValue(ColType.Response, ExtColProperty.Thresh, It.IsAny<double>(), out tempValue), Times.Once);
        }
        [Test]
        public void UpdateRangesFromData_HasBeenInitializedFalse_GetNewValueFunctionFired()
        {
            //Arrange
            _analysis.HasBeenInitialized = false;
            //Act
            _analysis.UpdateRangesFromData();
            //Assert
            var tempValue = 0.0;
            _data.Verify(gnv => gnv.GetNewValue(ColType.Flaw, ExtColProperty.Max, out tempValue), Times.Once);
            _data.Verify(gnv => gnv.GetNewValue(ColType.Flaw, ExtColProperty.Min, out tempValue), Times.Once);
            _data.Verify(gnv => gnv.GetNewValue(ColType.Response, ExtColProperty.Max, out tempValue), Times.Once);
            _data.Verify(gnv => gnv.GetNewValue(ColType.Response, ExtColProperty.Min, out tempValue), Times.Once);
            _data.Verify(gnv => gnv.GetNewValue(ColType.Response, ExtColProperty.Thresh, out tempValue), Times.Once);
        }


        /// <summary>
        /// Test for the public void  public void RunOnlyFitAnalysis() function
        /// </summary>
        /// RunOnlyFitAnalysis for HitMiss analyses
        [Test]
        public void RunOnlyFitAnalysis_AnalysisTypeHitMissNotInverse_ReturnsAnHMAnalysisObjectResultsToData()
        {
            using (EventRaisingStreamWriter outputWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("outputWriter"))))
            using (EventRaisingStreamWriter errorWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("errorWriter"))))
            {
                //Arrange
                SetPythonAndREngines();
                StandardHitMissSetup(TransformTypeEnum.Linear, outputWriter, errorWriter);
                //Act
                _analysis.RunOnlyFitAnalysis(_analysisBackendControl.Object);
                //Assert
                _analysisBackendControl.Verify(etHM => etHM.ExecuteAnalysisTransforms_HM(), Times.Once);
                _data.VerifySet(data => data.HMAnalysisObject = It.IsAny<HMAnalysisObject>(), Times.Once);
            }
            
        }
        [Test]
        public void RunOnlyFitAnalysis_AnalysisTypeHitMissIsInverse_ReturnsAnHMAnalysisObjectResultsToDataAndSkipsCheckingForRankedSetSampling()
        {
            using (EventRaisingStreamWriter outputWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("outputWriter"))))
            using (EventRaisingStreamWriter errorWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("errorWriter"))))
            {
                //Arrange
                SetPythonAndREngines();
                StandardHitMissSetup(TransformTypeEnum.Inverse, outputWriter, errorWriter);
                //Act
                _analysis.RunOnlyFitAnalysis(_analysisBackendControl.Object);
                //Assert
                _analysisBackendControl.Verify(etHM => etHM.ExecuteReqSampleAnalysisTypeHitMiss(), Times.Once);
                _data.VerifySet(data => data.HMAnalysisObject = It.IsAny<HMAnalysisObject>(), Times.Once);
            }

        }
       
        [Test]
        [TestCase(TransformTypeEnum.Linear)]
        [TestCase(TransformTypeEnum.Inverse)]
        public void RunOnlyFitAnalysis_AnalysisTypeHitMissThrowsException_DoesNotOverwriteTheHMAnalysisObject(TransformTypeEnum transformType)
        {
            using (EventRaisingStreamWriter outputWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("outputWriter"))))
            using (EventRaisingStreamWriter errorWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("errorWriter"))))
            {
                //Arrange
                SetPythonAndREngines();
                StandardHitMissSetup(transformType, outputWriter, errorWriter);
                _analysisBackendControl.Setup(abc => abc.ExecuteAnalysisTransforms_HM()).Throws<Exception>();
                _analysisBackendControl.Setup(abc => abc.ExecuteReqSampleAnalysisTypeHitMiss()).Throws<Exception>();
                //Act
                _analysis.RunOnlyFitAnalysis(_analysisBackendControl.Object);
                //Assert
                _data.VerifySet(data => data.HMAnalysisObject = It.IsAny<HMAnalysisObject>(), Times.Never);
            }

        }
        private void StandardHitMissSetup(TransformTypeEnum transform, EventRaisingStreamWriter outputWriter, EventRaisingStreamWriter errorWriter)
        {
            _data.SetupGet(data => data.HMAnalysisObject).Returns(new HMAnalysisObject("Sample Analysis"));
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.HitMiss;
            _analysis.InFlawTransform = transform;
            _python.Setup(p => p.OutputWriter).Returns(outputWriter);
            _python.Setup(p => p.ErrorWriter).Returns(errorWriter);
            _analysisBackendControl.Setup(abc => abc.HMAnalsysResults).Returns(new HMAnalysisObject("Output Analysis"));
        }
        // RunOnlyFitAnalysis for AHAT analyses
        [Test]
        public void RunOnlyFitAnalysis_AnalysisTypeAHat_ReturnsAnAHatAnalysisObjectResultsToData()
        {
            using (EventRaisingStreamWriter outputWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("outputWriter"))))
            using (EventRaisingStreamWriter errorWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("errorWriter"))))
            {
                //Arrange
                SetPythonAndREngines();
                StandardAHatSetup(TransformTypeEnum.Linear, outputWriter, errorWriter);
                //Act
                _analysis.RunOnlyFitAnalysis(_analysisBackendControl.Object);
                //Assert
                _analysisBackendControl.Verify(etHM => etHM.ExecuteAnalysisTransforms(), Times.Once);
                _data.VerifySet(data => data.AHATAnalysisObject = It.IsAny<AHatAnalysisObject>(), Times.Once);
            }

        }
        [Test]
        public void RunOnlyFitAnalysis_AnalysisTypeAHatThrowsException_DoesNotOverwriteTheAHatAnalysisObject()
        {
            using (EventRaisingStreamWriter outputWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("outputWriter"))))
            using (EventRaisingStreamWriter errorWriter = new EventRaisingStreamWriter(new MemoryStream(Encoding.UTF8.GetBytes("errorWriter"))))
            {
                //Arrange
                SetPythonAndREngines();
                StandardAHatSetup(TransformTypeEnum.Linear, outputWriter, errorWriter);
                _analysisBackendControl.Setup(abc => abc.ExecuteAnalysisTransforms()).Throws<Exception>();
                //Act
                _analysis.RunOnlyFitAnalysis(_analysisBackendControl.Object);
                //Assert
                _data.VerifySet(data => data.AHATAnalysisObject = It.IsAny<AHatAnalysisObject>(), Times.Never);
            }

        }
        private void StandardAHatSetup(TransformTypeEnum transform, EventRaisingStreamWriter outputWriter, EventRaisingStreamWriter errorWriter)
        {
            _data.SetupGet(data => data.AHATAnalysisObject).Returns(new AHatAnalysisObject("Sample Analysis"));
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            _analysis.InFlawTransform = transform;
            _python.Setup(p => p.OutputWriter).Returns(outputWriter);
            _python.Setup(p => p.ErrorWriter).Returns(errorWriter);
            _analysisBackendControl.Setup(abc => abc.AHatAnalysisResults).Returns(new AHatAnalysisObject("Output Analysis"));
        }

        /// <summary>
        /// tests for GenerateFileName()
        /// Will treat the function calls as fields since they are only one line of code each
        /// </summary>
        [Test]
        public void GenerateFileName_AnalysisTypeHitMiss_ReturnsHitMissFileNameString()
        {
            //Arrange
            _analysis.Operator = "operator";
            _analysis.SpecimenSet = "specimen set";
            _data.SetupGet(dt => dt.DataType).Returns(AnalysisDataTypeEnum.HitMiss);
            //Act
            string result = _analysis.GenerateFileName();
            //Assert
            Assert.That(result.Contains(_analysis.Operator));
            Assert.That(result.Contains(_analysis.SpecimenSet));
            Assert.That(result.Contains(_data.Object.DataType.ToString()));
        }
        [Test]
        public void GenerateFileName_AnalysisTypeAHat_ReturnsHitMissFileNameString()
        {
            //Arrange
            _analysis.Operator = "operator";
            _analysis.SpecimenSet = "specimen set";
            _analysis.Instrument = "instrument";
            _data.SetupGet(dt => dt.DataType).Returns(AnalysisDataTypeEnum.AHat);
            //Act
            string result = _analysis.GenerateFileName();
            //Assert
            Assert.That(result.Contains(_analysis.Operator));
            Assert.That(result.Contains(_analysis.SpecimenSet));
            Assert.That(result.Contains(_analysis.Instrument));
            Assert.That(result.Contains(_data.Object.DataType.ToString()));
        }

        /// <summary>
        /// tests for ClearAllEvents()
        /// </summary>
        [Test]
        public void ClearAllEvents_PythonIsNull_AnalysisDoneNotNull()
        {
            //Arrange
            _analysis.AnalysisDone += (sender, args) => { };
            //Act
            _analysis.ClearAllEvents();
            //Assert
            Assert.That(_analysis.AnalysisDone, Is.Not.Null);
        }
        [Test]
        public void ClearAllEvents_PythonIsNotNullAndEventsNeedsToBeClearedIsNull_AnalysisDoneIsNullAndEventsToBeClearedNotInvoked()
        {
            //Arrange
            SetPythonAndREngines();
            var events = SetupAnalysisDoneAndEventsNeedtoBeCleared();
            _analysis.EventsNeedToBeCleared = null;
            //Act
            _analysis.ClearAllEvents();
            //Assert
            Assert.That(_analysis.AnalysisDone, Is.Null);
            Assert.That(events, Is.Not.Null);
        }
        [Test]
        public void ClearAllEvents_PythonIsNotNullAndEventsNeedsToBeClearedIsNotNull_AnalysisDoneIsNullAndEventsToBeClearedInvoked()
        {
            //Arrange
            SetPythonAndREngines();
            var events = SetupAnalysisDoneAndEventsNeedtoBeCleared();
            //Act
            _analysis.ClearAllEvents();
            //Assert
            Assert.That(_analysis.AnalysisDone, Is.Null);
            Assert.That(events, Is.Not.Null);
        }
        private EventArgs SetupAnalysisDoneAndEventsNeedtoBeCleared()
        {
            _analysis.AnalysisDone += (sender, args) => { };
            var events = EventArgs.Empty;
            _analysis.EventsNeedToBeCleared += (sender, args) => { events = args; };
            return events;
        }


        /// <summary>
        /// skipping public void GetProjectInfo(out string fileName, out DataTable data) for now (need to figure out how to deal with the GetProjectInfoArgs dependency)
        /// </summary>

        /// <summary>
        /// tests for ExportProjectToExcel() function
        /// </summary>
        [Test]
        public void ExportProjectToExcel_ExportProjectIsNotNull_ExportProjectInvoked()
        {
            //Arrange
            var events = EventArgs.Empty;
            _analysis.ExportProject += (sender, args) => { events = args; };
            //_analysis.ExportProject = null;
            //Act
            _analysis.ExportProjectToExcel();
            //Assert
            Assert.That(events, Is.Null);
        }
        [Test]
        public void ExportProjectToExcel_ExportProjectIsNull_ExportProjectInvoked()
        {
            //Arrange
            var events = EventArgs.Empty;
            _analysis.ExportProject += (sender, args) => { events = args; };
            _analysis.ExportProject = null;
            //Act
            _analysis.ExportProjectToExcel();
            //Assert
            Assert.That(events, Is.Not.Null);
        }

        /// <summary>
        /// test for ToolTipText property
        /// </summary>
        [Test]
        public void ToolTipText_AnalysisTypeHitMissAndLengthIsLessThan40_ReturnsAStringWithFullNameAndHitMissParameters()
        {
            //Arrange
            SetUpHitMissParameters();
            //Act
            var result = _analysis.ToolTipText;
            //Assert
            Assert.That(result.Contains(_analysis.Name));
            AssertHitMissMetrics(result);
        }
        /// <summary>
        /// test for ToolTipText property
        /// </summary>
        [Test]
        public void ToolTipText_AnalysisTypeHitMissAndLengthIsGREATERThan40_ReturnsAStringWithTruncatedNameAndHitMissParameters()
        {
            //Arrange
            SetUpHitMissParameters();
            _analysis.Name = "ThisAnalysisNameIsLongerThan40Characters!";
            //Act
            var result = _analysis.ToolTipText;
            //Assert
            Assert.That(result.Contains(_analysis.Name), Is.False);
            Assert.That(result.Contains(_analysis.Name.Substring(0, 40)), Is.True);
            AssertHitMissMetrics(result);
        }
        private void SetUpHitMissParameters()
        {
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.HitMiss;
            _data.SetupGet(data => data.DataType).Returns(AnalysisDataTypeEnum.HitMiss);
            _analysis.InHitMissModel = HitMissRegressionType.LogisticRegression;
            _analysis.InConfIntervalType = ConfidenceIntervalTypeEnum.StandardWald;
            _analysis.InSamplingType = SamplingTypeEnum.SimpleRandomSampling;
            _data.SetupGet(afu => afu.AvailableFlawUnits).Returns( new List<string>() { "centimeters" });
        }
        private void AssertHitMissMetrics(string result)
        {
            Assert.That(result.Contains(_analysis.InHitMissModel.ToString()));
            Assert.That(result.Contains(_analysis.InConfIntervalType.ToString()));
            Assert.That(result.Contains(_analysis.InSamplingType.ToString()));
        }
        [Test]
        public void ToolTipText_AnalysisTypeAHatAndLengthIsLessThan40AndNotBoxCox_ReturnsAStringWithFullNameAndAHatParameters()
        {
            //Arrange
            SetUpAHatParameters(TransformTypeEnum.Log);
            //Act
            var result = _analysis.ToolTipText;
            //Assert
            AssertAHatMetrics(result);
            Assert.That(result.Contains(_analysis.InLambdaValue.ToString()), Is.False);
        }
        [Test]
        public void ToolTipText_AnalysisTypeAHatAndLengthIsLessThan40IsBoxCox_ReturnsAStringWithFullNameAndAHatParameters()
        {
            //Arrange
            SetUpAHatParameters(TransformTypeEnum.BoxCox);
            //Act
            var result = _analysis.ToolTipText;
            //Assert
            AssertAHatMetrics(result);
            Assert.That(result.Contains(_analysis.InLambdaValue.ToString()), Is.True);
        }
        [Test]
        [TestCase(TransformTypeEnum.Log)]
        [TestCase(TransformTypeEnum.BoxCox)]
        public void ToolTipText_AnalysisTypeAHatAndLengthIsLongerThan40_ReturnsAStringWithFullNameAndAHatParameters(TransformTypeEnum transform)
        {
            //Arrange
            SetUpAHatParameters(transform);
            _analysis.Name = "ThisAnalysisNameIsLongerThan40Characters!";
            //Act
            var result = _analysis.ToolTipText;
            //Assert
            AssertAHatMetrics(result);
            Assert.That(result.Contains(_analysis.Name), Is.False);
            Assert.That(result.Contains(_analysis.Name.Substring(0, 40)), Is.True);
            AssertAHatMetrics(result);
        }
        private void SetUpAHatParameters(TransformTypeEnum transform)
        {
            _analysis.AnalysisDataType = AnalysisDataTypeEnum.AHat;
            _data.SetupGet(data => data.DataType).Returns(AnalysisDataTypeEnum.AHat);
            _analysis.InResponseTransform = transform;
            _data.Setup(data => data.AHATAnalysisObject).Returns(new AHatAnalysisObject("Sample Analysis"));
            _analysis.InLambdaValue = 0.5;
            _analysis.InResponseDecision =5.0;
            _data.SetupGet(afu => afu.AvailableFlawUnits).Returns(new List<string>() { "centimeters" });
            _data.SetupGet(afu => afu.AvailableResponseUnits).Returns(new List<string>() { "amps" });
        }
        private void AssertAHatMetrics(string result)
        {
            Assert.That(result.Contains(_analysis.InResponseTransform.ToString()));
            Assert.That(result.Contains(_analysis.InResponseDecision.ToString()));
        }
        /// <summary>
        /// test for ShortName property
        /// The names passed in for short name will always contain '.'. Thus there is always an assertion the original input has at least one period.
        /// </summary>
        [Test]
        public void ShortName_ShortNameDoesNotStartWithSourceNameOrFlawName_ReturnsShortNameAsASubStringOfSourceName()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.Name = "test.flaw.response.MyProject";
            _data.SetupGet(afu => afu.AvailableFlawNames).Returns(new List<string>() { "flawName.centimeters" });
            DataSource source = new DataSource("MyDataSource", "ID", "flawName.centimeters", "Response");
            _analysis.HasBeenInitialized = true;
            _analysis.SetDataSource(source);
            //Act
            var result = _analysis.ShortName;
            //Assert
            Assert.That(_analysis.Name.Contains("."));
            Assert.That(result.StartsWith(_analysis.SourceName), Is.False);
            Assert.That(result.StartsWith(_analysis.FlawName), Is.False);
            Assert.That(result, Is.EqualTo(_analysis.Name));
        }
        [Test]
        public void ShortName_ShortNameStartsWithSourceNameButNotFlawName_ReturnsShortNameAsASubStringOfSourceName()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.Name = "test.flaw.response.MyProject";
            _data.SetupGet(afu => afu.AvailableFlawNames).Returns(new List<string>() { "flawName.centimeters" });
            DataSource source = new DataSource("test.flaw.response", "ID", "flawName.centimeters", "Response");
            _analysis.HasBeenInitialized = true;
            _analysis.SetDataSource(source);
            //Act
            var result = _analysis.ShortName;
            //Assert
            Assert.That(_analysis.Name.Contains("."));
            OverwrittenShortNameAssertions(result);
        }
        [Test]
        public void ShortName_ShortNameStartsWithFlawNameButNotWithSourceName_ReturnsShortNameAsASubStringOfSourceName()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.Name = "flawName.centimeters.MyProject";
            _data.SetupGet(afu => afu.AvailableFlawNames).Returns(new List<string>() { "flawName.centimeters" });
            DataSource source = new DataSource("test.flaw.response", "ID", "flawName.centimeters", "Response");
            _analysis.HasBeenInitialized = true;
            _analysis.SetDataSource(source);
            //Act
            var result = _analysis.ShortName;
            //Assert
            Assert.That(_analysis.Name.Contains("."));
            OverwrittenShortNameAssertions(result);
        }
        [Test]
        public void ShortName_ShortNameStartsBothSourceNameAndThenFlawName_ReturnsShortNameAsASubStringOfSourceName()
        {
            //Arrange
            SetPythonAndREngines();
            _analysis.Name = "test.flaw.response.flawName.centimeters.MyProject";
            _data.SetupGet(afu => afu.AvailableFlawNames).Returns(new List<string>() { "flawName.centimeters" });
            DataSource source = new DataSource("test.flaw.response", "ID", "flawName.centimeters", "Response");
            _analysis.HasBeenInitialized = true;
            _analysis.SetDataSource(source);
            //Act
            var result = _analysis.ShortName;
            //Assert
            Assert.That(_analysis.Name.Contains("."));
            OverwrittenShortNameAssertions(result);
        }
        private void OverwrittenShortNameAssertions(string result)
        {
            Assert.That(result.StartsWith(_analysis.SourceName), Is.False);
            Assert.That(result.StartsWith(_analysis.FlawName), Is.False);
            Assert.That(result, Is.Not.EqualTo(_analysis.Name));
        }

        /// <summary>
        /// test for RaiseCreatedAnalyis(Analysis clone) property
        /// </summary>
        [Test]
        public void RaiseCreatedAnalysis_CreatedAnalysisIsNull_NotInvoked()
        {
            //Arrange
            var events = AnalysisListArg.Empty;
            _analysis.CreatedAnalysis += (sender, args) => { events = args; };
            _analysis.CreatedAnalysis = null;
            //Act
            _analysis.RaiseCreatedAnalysis(_analysis);
            //Assert 
            Assert.That(events, Is.EqualTo(AnalysisListArg.Empty));
            Assert.That(events as AnalysisListArg, Is.Null);
        }

        [Test]
        public void RaiseCreatedAnalysis_CreatedAnalysisIsNotNull_EventInvokedAndArgsContainAnalysisList()
        {
            //Arrange
            EventArgs events = AnalysisListArg.Empty;
            _analysis.CreatedAnalysis += (sender, args) => { events = args; };
            //Act
            _analysis.RaiseCreatedAnalysis(_analysis);
            //Assert 
            Assert.That(events, Is.Not.EqualTo(AnalysisListArg.Empty));
            Assert.That(events as AnalysisListArg, Is.Not.Null);
            Assert.That(((AnalysisListArg)events).Analyses.Count, Is.GreaterThanOrEqualTo(1));
        }
    }
}
