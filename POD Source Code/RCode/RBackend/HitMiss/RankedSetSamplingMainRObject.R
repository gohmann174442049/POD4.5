#     Probability of Detection Version 4.5 (PODv4.5)
#     Copyright (C) 2022  University of Dayton Research Institute (UDRI)
# 
#     This program is free software: you can redistribute it and/or modify
#     it under the terms of the GNU General Public License as published by
#     the Free Software Foundation, either version 3 of the License, or
#     (at your option) any later version.
# 
#     This program is distributed in the hope that it will be useful,
#     but WITHOUT ANY WARRANTY; without even the implied warranty of
#     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
#     GNU General Public License for more details.
# 
#     You should have received a copy of the GNU General Public License
#     along with this program.  If not, see <https://www.gnu.org/licenses>

# This is the main class used to control and execute ranked set sampling analyses
# Ranked Set Sampling R Object Class

# parameters:
# dataFrme = the original input hit/miss dataframe
# rssComponentFromMain = the class containing the components unique to ranked set sampling (max resamples, m, r, regressionType, includeNAInput)
# CITypeRSS = the confidence interval to be applied to the ranked set samples
# normSampleAmount = also known as a_x_n, this is the number of normally distributed crack sizes to generate
# medianAValues = the output a values found by taking the median of the critical a values found for each ranked set sample
# pODDataFrame = the output ranked set sample dataframe to be outputted to the UI
# medianResidDataframe = the output ranked set sample residual dataframe to be outputted to UI
# covarMatrix = the median values of the variance-covariance matrices found for each ranked set sample
# goodnessOfFit = the median values of the goodnessOfFits found for each ranked set sample

RSSMainClassObject <- setRefClass("RSSMainClassObject", fields = list(dataFrame="data.frame",
                                                                      rsSComponentFromMain= "RSSComponents",
                                                                      CITypeRSS="character",
                                                                      normSampleAmount="numeric",
                                                                      medianAValues="list",
                                                                      pODDataFrame="data.frame",
                                                                      medianResidDataframe="data.frame",
                                                                      covarMatrix="matrix",
                                                                      goodnessOfFit="numeric"),
                                                        methods = list(
                                                        setMedianAValues=function(psMedianAValues){
                                                          medianAValues<<-psMedianAValues
                                                        },
                                                        getMedianAValues=function(){
                                                          return(medianAValues)
                                                        },
                                                        setPODDataFrame=function(psPODDataFrame){
                                                          pODDataFrame<<-psPODDataFrame
                                                        },
                                                        getPODDataFrame=function(){
                                                          return(pODDataFrame)
                                                        },
                                                        setMedianResidDataframe=function(psMedianResidDataframe){
                                                          medianResidDataframe<<-psMedianResidDataframe
                                                        },
                                                        getMedianResidDataframe=function(){
                                                          return(medianResidDataframe)
                                                        },
                                                        getMedianCovarMatrix=function(){
                                                          return(covarMatrix)
                                                        },
                                                        setMedianCovarMatrix=function(psCovarMatrix){
                                                          covarMatrix<<-psCovarMatrix
                                                        },
                                                        setGoodnessOfFit=function(psGoodFit){
                                                          goodnessOfFit<<-psGoodFit
                                                        },
                                                        getGoodnessOfFit=function(){
                                                          return(goodnessOfFit)
                                                        },
                                                        executeRSSPOD=function(){
                                                          if(CITypeRSS=="ModifiedWald" || CITypeRSS=="LR" || CITypeRSS=="MLR"){
                                                            simCracks=genModifiedWaldDataset()
                                                          }
                                                          #generate the logit data and store the results in the environment
                                                          newLogitSubsetsGen=RankedSetRegGen$new()
                                                          newLogitSubsetsGen$initialize(testDataInput=dataFrame,
                                                                                      maxResamples=rsSComponentFromMain$maxResamples,
                                                                                      set_mInput=rsSComponentFromMain$set_m,
                                                                                      set_rInput=rsSComponentFromMain$set_r,
                                                                                      regTypeInput=rsSComponentFromMain$regressionType)
                                                          newLogitSubsetsGen$generateFullRSS()
                                                          RankedResultsSet=newLogitSubsetsGen$getRankedSetResults()
                                                          
                                                          #execute class and return logit results
                                                          #generate POD curve and store key values for a25,a50,a90, sigma, and a9095
                                                          logitResultsSet=newLogitSubsetsGen$getRSSLogitResults()
                                                          checkFailures=newLogitSubsetsGen$countBadLogits()
                                                          #create the residual table based on the median of the regression results
                                                          genResidualDataframe(logitResultsSet)
                                                          #if at least one logit model converged, generate a pod curve
                                                          if(checkFailures==FALSE){
                                                            newPODCurve<-GenPODCurveRSS$new()
                                                            #Standard wald
                                                            if(CITypeRSS=="StandardWald"){
                                                              # newPODCurve$initialize(logitResultsPODInput=logitResultsSet,
                                                              #                        excludeNAInput=rsSComponentFromMain$excludeNA,
                                                              #                        RSSDataFrames=RankedResultsSet)
                                                              # newPODCurve$genMainPODDFSDWald()
                                                              newPODCurve$initialize(logitResultsPODInput=logitResultsSet,
                                                                                     excludeNAInput=rsSComponentFromMain$excludeNA,
                                                                                     RSSDataFrames=RankedResultsSet,
                                                                                     origDataSetInput=dataFrame)
                                                              newPODCurve$newWaldGen()
                                                            }
                                                            #Modified wald
                                                            else if(CITypeRSS=="ModifiedWald"){
                                                              newPODCurve$initialize(logitResultsPODInput=logitResultsSet, 
                                                                                     excludeNAInput=rsSComponentFromMain$excludeNA,
                                                                                     RSSDataFrames=RankedResultsSet,
                                                                                     simCrackSizes=simCracks)
                                                              newPODCurve$genPODModWald()
                                                            }
                                                            else if(CITypeRSS=="LR"){
                                                              newPODCurve$initialize(logitResultsPODInput=logitResultsSet, 
                                                                                     excludeNAInput=rsSComponentFromMain$excludeNA,
                                                                                     RSSDataFrames=RankedResultsSet,
                                                                                     simCrackSizes=simCracks)
                                                              newPODCurve$genPODLR()
                                                            }
                                                            else if(CITypeRSS=="MLR"){
                                                              newPODCurve$initialize(logitResultsPODInput=logitResultsSet, 
                                                                                     excludeNAInput=rsSComponentFromMain$excludeNA,
                                                                                     RSSDataFrames=RankedResultsSet,
                                                                                     simCrackSizes=simCracks)
                                                              newPODCurve$genPODMLR()
                                                            }
                                                            setGoodnessOfFit(newPODCurve$getGoodnessOfFit())
                                                            setMedianCovarMatrix(newPODCurve$getMedianCovarMatrix())
                                                            setPODDataFrame(newPODCurve$getPODCurveDF())
                                                            genAValues(logitResultsSet, RankedResultsSet)
                                                          }
                                                          else{
                                                            print("all logit models failed to converge!")
                                                          }
                                                          
                                                        },
                                                        genModifiedWaldDataset=function(){
                                                          #generate normally distributed crack sizes
                                                          newNormalCrackDist=GenNormFit$new(cracks=dataFrame$x, sampleSize=normSampleAmount, 
                                                                                            Nsample=nrow(dataFrame))
                                                          newNormalCrackDist$genNormalFit()
                                                          simCracks=newNormalCrackDist$getSimCrackSizesArray()
                                                          x = sort(simCracks)[seq(from=1,to=length(simCracks),
                                                                                  length.out=normSampleAmount)]
                                                          return(x)
                                                        },
                                                        genAValues=function(logitResults, RankedResults){
                                                          newAValuesCalc=GenRSS_A_Values$new()
                                                          newAValuesCalc$initialize(pODRSSDFInput=getPODDataFrame(), 
                                                                                    excludeNAInput= rsSComponentFromMain$excludeNA,
                                                                                    logitResultsListInput=logitResults,
                                                                                    RSSDataFramesInput=RankedResults)
                                                          if(CITypeRSS=="StandardWald"){
                                                            #newAValuesCalc$genAValueswald()
                                                            newAValuesCalc$genAValuesStandWald()
                                                          }
                                                          else if(CITypeRSS=="ModifiedWald"){
                                                            newAValuesCalc$genAValuesModWald()
                                                          }
                                                          else if(CITypeRSS=="LR"){
                                                            newAValuesCalc$genAValuesLR()
                                                          }
                                                          else if(CITypeRSS=="MLR"){
                                                            newAValuesCalc$genAValuesMLR()
                                                          }
                                                          setMedianAValues(newAValuesCalc$getAValuesList())
                                                          
                                                        },
                                                        genResidualDataframe=function(logitResultsSet){
                                                          beta0List=c()
                                                          beta1List=c()
                                                          for (i in 1:length(logitResultsSet)){
                                                            beta0List=c(beta0List, logitResultsSet[[i]]$coefficients[[1]])
                                                            beta1List=c(beta1List, logitResultsSet[[i]]$coefficients[[2]])
                                                          }
                                                          medianBeta0=median(beta0List)
                                                          medianBeta1=median(beta1List)
                                                          t_transVec=c()
                                                          for (i in 1:nrow(dataFrame)){
                                                            exponentVal=medianBeta0+medianBeta1*dataFrame$x[i]
                                                            currAns=exp(exponentVal)/(1+exp(exponentVal))
                                                            t_transVec=c(t_transVec, currAns)
                                                          }
                                                          makeResidualTable=data.frame(
                                                            flaw=dataFrame$x,
                                                            transformFlaw= integer(nrow(dataFrame)),
                                                            hitrate= dataFrame$y,
                                                            t_trans= t_transVec,
                                                            diff=dataFrame$y-t_transVec
                                                          )
                                                          setMedianResidDataframe(makeResidualTable)
                                                        }
                                                       ))
