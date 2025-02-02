﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using POD.Data;

namespace POD.Controls
{
    public partial class CensoredLinearityChart : LinearityChart
    {
        double _slope = 0.0;
        double _intercept = 0.0;
        double _fitError = 0.0;
        double _repeatError = 0.0;

        public CensoredLinearityChart()
        {
            InitializeComponent();

            //all charts should be added through SetupChart()
            Series.Clear();
            Legends.Clear();
        }

        public override void SetupChart(string flawName, string flawUnit, List<string> responseNames, List<string> responseUnits)
        {
            base.SetupChart(flawName, flawUnit, responseNames, responseUnits);

            Series series;

            this.YAxis.Title = "Residuals";

            if (Series.FindByName(LinearityChartLabels.CompleteCensored) == null)
            {
                //draw residual for completely censored data
                Series.Add(new Series(LinearityChartLabels.CompleteCensored));
                series = Series.Last();
                series.ChartType = SeriesChartType.Point;
                series.YValuesPerPoint = 1;
                series.Color = Color.FromArgb(ChartColors.LineAlpha, ChartColors.CensoredPoints);
                series.MarkerStyle = MarkerStyle.Circle;
                series.IsVisibleInLegend = false;
            }

            if (Series.FindByName(LinearityChartLabels.PartialCensored) == null)
            {
                //draw residual for partially censored data
                Series.Add(new Series(LinearityChartLabels.PartialCensored));
                series = Series.Last();
                series.ChartType = SeriesChartType.Point;
                series.YValuesPerPoint = 1;
                series.Color = Color.FromArgb(ChartColors.LineAlpha, ChartColors.SemiCensoredPoints);
                series.MarkerStyle = MarkerStyle.Circle;
                series.IsVisibleInLegend = false;
            }

            //add legend to explain different colored points
            if (Legends.FindByName(LinearityChartLabels.LegendTitle) == null)
            {
                Legends.Add(new Legend(LinearityChartLabels.LegendTitle));
                Legend legend = Legends.Last();
                legend.Alignment = StringAlignment.Center;
                legend.Docking = Docking.Top;
            }

            AutoNameYAxis = false;
            YAxisNameIsUnitlessConstant = true;
            YAxisTitle = "Residuals";
            XAxisTitle = flawName;
            XAxisUnit = flawUnit;
            ChartTitle = "Residuals";
        }

        public override void ClearEverythingButPoints()
        {
            base.ClearEverythingButPoints();

            Series[LinearityChartLabels.CompleteCensored].Points.Clear();
            Series[LinearityChartLabels.PartialCensored].Points.Clear();
        }

        public Series LeftCensor
        {
            get
            {
                return Series[LinearityChartLabels.LeftCensor];
            }
        }

        public Series RightCensor
        {
            get
            {
                return Series[LinearityChartLabels.RightCensor];
            }
        }

        public Series CompleteCensored
        {
            get
            {
                return Series[LinearityChartLabels.CompleteCensored];
            }
        }

        public Series PartialCensored
        {
            get
            {
                return Series[LinearityChartLabels.PartialCensored];
            }
        }

        public override string TooltipText
        {
            get
            {
                var text = "";

                text += "Linear Fit Parameters" + Environment.NewLine;
                text += "Slope:\t\t" + _slope.ToString("F3") + Environment.NewLine;
                text += "Intercept:\t" + _intercept.ToString("F3") + Environment.NewLine;
                text += "Residual Error:\t" + _fitError.ToString("F3") + Environment.NewLine;
                text += "Repeat Error:\t" + _repeatError.ToString("F3");

                return text;
            }
        }

        public void FillChart(IAnalysisData myData, double mySlope, double myIntercept, double fitError, double repeatError)
        {
            double xMin = myData.InvertTransformedFlaw(myData.UncensoredFlawRangeMin);
            double xMax = myData.InvertTransformedFlaw(myData.UncensoredFlawRangeMax);

            FlawEstimate.Points.Clear();
            FlawEstimate.Points.AddXY(xMin, 0.0);
            FlawEstimate.Points.AddXY(xMax, 0.0);

            _slope = mySlope;
            _intercept = myIntercept;
            _fitError = fitError;
            _repeatError = repeatError;

            DataView view = myData.ResidualUncensoredTable.DefaultView;
            Uncensored.Points.DataBindXY(view, "flaw", view, "t_diff");

            var uncensoredMax = Double.NegativeInfinity;
            var uncensoredMin = Double.PositiveInfinity;
            //TODO: fix this when tranform log of y
            if (Uncensored.Points.Count > 0)
            {
                uncensoredMax = Uncensored.Points.FindMaxByValue("Y1").YValues[0];
                uncensoredMin = Uncensored.Points.FindMinByValue("Y1").YValues[0];
            }

            view = myData.ResidualPartialCensoredTable.DefaultView;
            PartialCensored.Points.DataBindXY(view, "flaw", view, "t_diff");

            var censoredMax = Double.NegativeInfinity;
            var censoredMin = Double.PositiveInfinity;

            if (PartialCensored.Points.Count > 0)
            {
                censoredMax = PartialCensored.Points.FindMaxByValue("Y1").YValues[0];
                censoredMin = PartialCensored.Points.FindMinByValue("Y1").YValues[0];
            }

            var globalResponseMax = (uncensoredMax > censoredMax) ? uncensoredMax : censoredMax;
            var globalResponseMin = (uncensoredMin < censoredMin) ? uncensoredMin : censoredMin;

            if(globalResponseMax < globalResponseMin)
            {
                globalResponseMax = 1.0;
                globalResponseMin = -1.0;
            }

            AxisObject yAxis = new AxisObject();
            AxisObject xAxis = new AxisObject();

            AnalysisData.GetBufferedRange(this, xAxis, xMin, xMax, AxisKind.X);
            AnalysisData.GetBufferedRange(this, yAxis, globalResponseMin, globalResponseMax, AxisKind.Y);//myData.ResponseTransform == TransformTypeEnum.Linear);

            //xAxis.Interval /= 2.0;
            //yAxis.Interval /= 2.0;

            SetXAxisRange(xAxis, myData);
            SetYAxisRange(yAxis, myData);

            RelabelAxesBetter(xAxis, yAxis, null, null, Globals.GetLabelIntervalBasedOnChartSize(this, AxisKind.X), 
                              Globals.GetLabelIntervalBasedOnChartSize(this, AxisKind.Y), false, true);
          
        }
    }
}
