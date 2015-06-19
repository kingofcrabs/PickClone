#pragma once
#include "stdafx.h"

ref class CloneConstrain
{
public:
	int minSize;
	int maxSize;
};


class EngineImpl
{
public:
	void Load(std::string sFile);
	EngineImpl();
	std::string MarkClones(CloneConstrain^ constrains, std::vector<cv::Point>& centers);
private:
	void GetCircleROI(cv::Mat& src);
	void RemovePtsNotInROI(cv::Mat& src, CvPoint ptMass);
	CvPoint CalcuMassCenter(std::vector<cv::Point> contour);
	void  FindContours(const cv::Mat& thresholdImg,	std::vector<std::vector<cv::Point>>& contours,int min, int max = 999999);
	double  GetDistance(double x1, double y1, double x2, double y2);
	std::vector<std::vector<cv::Point>> MarkAllContoursGray(cv::Mat& src, cv::Mat& org);
	std::vector<std::vector<cv::Point>> MarkAllContours(cv::Mat& src, std::string filePath2Save);
	std::string workingFolder;
	cv::Mat img;
};

