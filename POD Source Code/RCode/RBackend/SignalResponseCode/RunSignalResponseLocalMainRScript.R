#options(warn=-1)
library(ggplot2) # gorgeous plots
library(gridExtra) # useful for plotting in grids
library(MASS) #contains boxcox and much more
#library(olsrr) #makes some nice plots to check assumptions
library(stats)
library(nlme) # contains gls = generalized least squares
library(pracma) #practical math...contains some functions from matlab
library(ggResidpanel)
library(carData)
library(car) # Need this for durbinWatsonTest
library(survival)
library(corrplot)
folderLocation=dirname(rstudioapi::getSourceEditorContext()$path)
source(paste(folderLocation, "/GenPODSignalResponeRObject.R", sep=""))
source(paste(folderLocation, "/SignalResponseMainAnalysisRObject.R", sep=""))
source(paste(folderLocation, "/PrepareDataWithMultipleResponsesRObject.R", sep=""))
source(paste(folderLocation, "/GenFrequencyTableR.R", sep = ""))
source(paste(folderLocation, "/RecalculatePODGhostCurve.R", sep = ""))

#data_obs = read.csv(paste(folderLocation,'/Plot_Data_50.csv',sep=""), header=TRUE, col.names=c("y","x"))

data_obs = read.csv(paste("C:/Users/gohmancm/Desktop/PODv4.5ExampleDataRepo/PODv4.5ExampleDatasets/aHat/dataFromPlots_exp.csv",sep=""), header=TRUE)

#data_obs = read.csv(paste("C:/Users/gohmancm/Desktop/PODv4.5ExampleDataRepo/PODv4.5ExampleDatasets/aHat/ahat_vs_a_1823_Example1_LogY.csv"), header=TRUE)
data_obs=na.omit(data_obs)
colnames(data_obs)[1] <- "Index"
colnames(data_obs)[2] <- "x"
data_obs$A11=data_obs$A12
colnames(data_obs)[3] <- "y"
#data_obs$A21=NULL
data_obs$A12=NULL
#data_obs$A13=NULL
#data_obs$A14=NULL
#data_obs$y2=NULL
#log both
#data_obs$y=log(data_obs$y)
#data_obs$y2=log(data_obs$y2)
begin<-proc.time()
###################################################### Uncomment this for boxcox transform (leave lambda in to prevent a varaible not found error)
#bc<-boxcox(data_obs$y~data_obs$x, plotit =FALSE)
lambda<-0
#data_obs$x=log(data_obs$x)
#lambda<-bc$x[which.max(bc$y)]
#lambda<- -2.0
#data_obs$y<-(data_obs$y^lambda-1)/lambda
##############################################################
#data_obs$event= c(2, 2, 1, 2, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0)
#data_obs$event= c(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 1, 0, 1, 0, 0, 0, 0)
#data_obs$event= c(1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1)
data_obs$event=rep(1,  nrow(data_obs))

fullAnalysis=TRUE
newSRAnalysis<-AHatAnalysis$new(signalRespDF=data_obs,y_dec=5, modelType=3, lambda=lambda)
newSRAnalysis$generateDefaultValues()
newSRAnalysis$executeAhatvsA()
linResults<-newSRAnalysis$getLinearModel()
results<-newSRAnalysis$getResults()
critPoints<-newSRAnalysis$getCritPts()
keyAValues<-newSRAnalysis$getKeyAValues()
slope<-newSRAnalysis$getModelSlope()
intercept<-newSRAnalysis$getModelIntercept()
testResults<-newSRAnalysis$getLinearTestResults()
r_squared<-newSRAnalysis$getRSquared()
stdErrors<-newSRAnalysis$getRegressionStdErrs()
covarMatrix<-newSRAnalysis$getCovarianceMatrix()
resDF<-newSRAnalysis$getResidualTable()
threshDF<-newSRAnalysis$getThresholdDF()
normalityDF<-newSRAnalysis$getFreqTable()

end<-proc.time()
print("total time:")
print(end-begin)


recalcPODClass<-RecalcOriginalPOD$new(signalRespDFFull=data_obs,y_dec=5, modelType=3, lambda=lambda, tau = newSRAnalysis$getTau())
recalcPODClass$recalcPOD(TRUE)


#newSRAnalysis$plotSimdata(results)
#newSRAnalysis$plotCI(results)
newSRAnalysis$plotNormality(normalityDF)





# full<-lm(y~x, data=data_obs)
# #constVector=rep(linearModel_lm$coefficients[[1]], nrow(data_obs))
# #partial<-lm(constVector~x, data= data_obs)
# #partial<-lm(1~x, data= data_obs)
# #partial$coefficients[[2]]=0
# #partial<-lm(y~poly(x, 2), data= data_obs)
# partial<-lm(y~1, data= data_obs)
# ANOVATable<-Anova(full, partial)
# pValue=1-ANOVATable$`Pr(>F)`[1]
# newSignalDF_Fit<-data.frame(x= data_obs$x, y=a.hat.vs.a.censored$linear.predictors)
# newCensoredLMObject<-lm(y~x, data=newSignalDF_Fit,na.action=na.omit)
# newCensoredLMObject$model$y=data_obs$y
# newResiduals=c()
# for(i in 1:length(newCensoredLMObject$fitted.values)){
#   residual=newCensoredLMObject$model$y[i]-newCensoredLMObject$fitted.values[i]
#   newResiduals=c(newResiduals, residual)
# }
# newCensoredLMObject$residuals=newResiduals
#boxCoxTEst<-boxcox(data_obs$y~data_obs$x)
#lambda <-boxCoxTEst$x[which.max(boxCoxTEst$y)]
