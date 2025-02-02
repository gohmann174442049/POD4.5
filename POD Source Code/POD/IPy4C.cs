﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Microsoft.Scripting.Hosting;
//using Microsoft.Scripting.Utils;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Data;
//using IronPython.Hosting;
using System.Diagnostics;
using CSharpBackendWithR;
namespace POD
{
    public class IPy4C : I_IPy4C
    {
        Dictionary<string, HMAnalysisObject> _hitMissAnalyses;
        Dictionary<string, AHatAnalysisObject> _ahatAnalyses;
        MemoryStream _outputStream;
        MemoryStream _errorStream;
        EventRaisingStreamWriter _outputWriter;
        //PODStatusBar _statusBar;
        public AnalysisErrorHandler OnAnalysisError { get; set; }
        public AnalysisErrorHandler OnProgressUpdate { get; set; }
        public AnalysisErrorHandler OnAnalysisFinish { get; set; }
        public string CurrentAnalysisName { get; set; }
        /// <summary>
        /// A way to at least record status even if the status bar isn't being used.
        /// </summary>
        string _simpleStatus = "";
        int _simpleProgress = 0;
        string _simpleError = "";

        public EventRaisingStreamWriter OutputWriter
        {
            get { return _outputWriter; }
        }
        EventRaisingStreamWriter _errorWriter;
        //remove***
        public int ProgressOutOf100
        {
            get
            {
                return _simpleProgress;
            }
            set
            {
                _simpleProgress = value;

                if (OnProgressUpdate != null)
                {
                    OnProgressUpdate.Invoke(this, new ErrorArgs(CurrentAnalysisName, value));
                }
            }
        }
        //remove***
        public string ProgressText
        {
            get
            {
                return _simpleStatus;
            }
            set
            {
                _simpleStatus = value;
            }
        }

        public void NotifyFinishAnalysis()
        {
            if (OnAnalysisFinish != null)
            {
                OnAnalysisFinish.Invoke(this, new ErrorArgs(CurrentAnalysisName, ""));
            }
        }

        public void AddErrorText(string myError)
        {
             _simpleError = _simpleError + "; " + myError;

            if(OnAnalysisError != null)
            {
                OnAnalysisError.Invoke(this, new ErrorArgs(CurrentAnalysisName, myError));
            }
        }

        public void ClearErrorText()
        {
            _simpleError = "";
        }

        public EventRaisingStreamWriter ErrorWriter
        {
            get { return _errorWriter; }
        }

        public IPy4C()
        {
            _outputStream = new MemoryStream();
            _errorStream = new MemoryStream();

            _outputWriter = new EventRaisingStreamWriter(_outputStream);
            _errorWriter = new EventRaisingStreamWriter(_errorStream);

            //used to store the hitmiss analyses
            _hitMissAnalyses = new Dictionary<string, HMAnalysisObject>();
            //used to store ahat analyses
            _ahatAnalyses = new Dictionary<string, AHatAnalysisObject>();
        }
        //object used for hit miss analyses
        public HMAnalysisObject HitMissAnalsysis(string myAnalysisName)
        {
            //if analysis name doesn't alredy exist in the dictionary create a new one
            if (_hitMissAnalyses.ContainsKey(myAnalysisName) == false)
            {
                //create a new hitmiss analysis object
                HMAnalysisObject hitMissAnalsyis = new HMAnalysisObject(myAnalysisName);
                _hitMissAnalyses.Add(myAnalysisName, hitMissAnalsyis);
                return (hitMissAnalsyis);
            }
            //if it does, simply return that def/key pair from the _hitMissAnalyses dictionary
            else
            {
                return _hitMissAnalyses[myAnalysisName];
            }

        }
        public AHatAnalysisObject AHatAnalysis(string myAnalysisName)
        {
            //if analysis name doesn't alredy exist in the dictionary create a new one
            if (_ahatAnalyses.ContainsKey(myAnalysisName) == false)
            {
                //create a new hitmiss analysis object
                AHatAnalysisObject aHatAnalsyis = new AHatAnalysisObject(myAnalysisName);
                _ahatAnalyses.Add(myAnalysisName, aHatAnalsyis);
                return (aHatAnalsyis);
            }
            //if it does, simply return that def/key pair from the _ahatAnalyses dictionary
            else
            {
                return _ahatAnalyses[myAnalysisName];
            }
        }

        //This function converts the transform type into an integer and returns it
        //The transformtypeEnum is located in Globals.cs
        public int TransformEnumToInt(TransformTypeEnum myTransformType)
        {
            int transform = 0;

            switch(myTransformType)
            {
                case TransformTypeEnum.Linear:
                    transform = 1;
                    break;
                case TransformTypeEnum.Log:
                    transform = 2;
                    break;
                case TransformTypeEnum.Inverse:
                    transform = 3;
                    break;
                case TransformTypeEnum.Exponetial:
                    transform = 4;
                    break;
                case TransformTypeEnum.BoxCox:
                    transform = 5;
                    break;
                default:
                    transform = 1;
                    break;
            }

            return transform;
        }
        //used to output the decision value
        //self.choices = ["P > 0.1", "0.05 < P <= 0.1", ".025 < P <= 0.05",
        //                "0.01 < P <= .025", ".005 < P <= 0.01", "P <= .005", "Undefined"]
        public string GetPValueDecision(double myValue)
        {
            if (myValue > .1)
                return "P > 0.1";
            else if (myValue > .05)
                return "0.05 < P <= 0.1";
            else if (myValue > .025)
                return ".025 < P <= 0.05";
            else if (myValue > .01)
                return "0.01 < P <= .025";
            else if (myValue > .005)
                return ".005 < P <= 0.01";
            else if (myValue >= 0.0)
                return "P <= .005";
            else
                return "Undefined";
        }
        //used to calculate the nth root given a double 'A'
        //source : https://stackoverflow.com/questions/55987201/calculating-the-cubic-root-for-negative-number
        //author : Tim ---- https://stackoverflow.com/users/6785695/tim
        public static double NthRoot(double A, double root, double checkLambdaDenominator=2.0)
        {
            if(checkLambdaDenominator % 2 != 0 && A < 0)
            {
                return -Math.Pow(-A, 1.0 / root);
            }
            else
            {
                return Math.Pow(A, 1.0 / root);
            }
        }
        static long gcd(long a, long b)
        {
            if (a == 0)
                return b;
            else if (b == 0)
                return a;
            if (a < b)
                return gcd(a, b % a);
            else
                return gcd(b, a % b);
        }

        // Function to convert decimal to fraction
        public static void DecimalToFraction(double number, out long numerator, out long denominator)
        {
            if (number >= 1)
            {
                throw new Exception("Error: input to decimal fraction conversion cannot be a mixed number");
            }
            else if(number < 0)
            {
                throw new Exception("Error: input to decimal fraction conversion cannot be a negative number");
            }
            // Fetch integral value of the decimal
            double intVal = Math.Floor(number);

            // Fetch fractional part of the decimal
            double fVal = number - intVal;

            // Consider precision value to
            // convert fractional part to
            // integral equivalent
            long pVal = 1000000000;

            // Calculate GCD of integral
            // equivalent of fractional
            // part and precision value
            long gcdVal = gcd((long)Math.Round(
                            fVal * pVal), pVal);

            // Calculate num and deno
            numerator= (long)Math.Round(fVal * pVal) / gcdVal;
            denominator= pVal / gcdVal;
        }
        /// <summary>
        /// This method gets the max precision of either the flaws, responses, or both
        /// for labeling the graphs.
        /// </summary>
        /// <param name="podItems"></param>
        public static int GetMaxPrecision(List<double> podItems)
        {
            int maxPrecision = 0;

            foreach(double item in podItems)
            {
                string numAsString = item.ToString();
                string[] splitDouble = numAsString.Split('.');
                int currPrecision;
                if (splitDouble.Length < 2)
                    currPrecision = 0;
                else
                    currPrecision = splitDouble[1].Length;
                if (currPrecision > maxPrecision)
                    maxPrecision = currPrecision;
            }

            return maxPrecision;
        }

        public static int GetMaxPrecisionDict(Dictionary<string, List<double>> podDictionary)
        {
            int maxPrecision = 0;

            foreach (List<double> item in podDictionary.Values)
            {
                int currPrecision= GetMaxPrecision(item);
                if (currPrecision > maxPrecision)
                    maxPrecision = currPrecision;
            }

            return maxPrecision;
        }
        public void ClearAnalyses()
        {
            _hitMissAnalyses.Clear();
            _ahatAnalyses.Clear();
        }
        public void Close()
        {
            
            try
            {
                _outputStream.Flush();
            }
            catch
            {

            }

            _outputStream.Close();

            try
            {
                _errorStream.Flush();
            }
            catch
            {

            }

            _errorStream.Close();

            try
            {
                _outputWriter.Flush();
            }
            catch
            {

            }

            try
            {
                _outputWriter.Close();
            }
            catch
            {

            }
        }
        public int FailedRunCount { get; set; }

        public int DictionarySizeHitMiss
        {
            get
            {
                return _hitMissAnalyses.Count;
            }
        }
        public int DictionarySizeSignalResponse
        {
            get
            {
                return _ahatAnalyses.Count;
            }
        }
    }
    public interface I_IPy4C
    {
        void AddErrorText(string myError);
        void ClearErrorText();
        EventRaisingStreamWriter ErrorWriter {get;}

        HMAnalysisObject HitMissAnalsysis(string myAnalysisName);
        AHatAnalysisObject AHatAnalysis(string myAnalysisName);

        int TransformEnumToInt(TransformTypeEnum myTransformType);

        string GetPValueDecision(double myValue);

        void Close();

        void NotifyFinishAnalysis();


        AnalysisErrorHandler OnAnalysisError { get; set; }
        AnalysisErrorHandler OnProgressUpdate { get; set; }
        AnalysisErrorHandler OnAnalysisFinish { get; set; }
        int FailedRunCount { get; set; }

        EventRaisingStreamWriter OutputWriter { get; }

        string ProgressText { get; set; }

        int ProgressOutOf100 { get; set; }

        string CurrentAnalysisName { get; set; }
    }
}
