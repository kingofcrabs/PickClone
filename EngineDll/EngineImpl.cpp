#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;

EngineImpl::EngineImpl()
{

}

CvPoint  EngineImpl::CalcuMassCenter(vector<cv::Point> contour)
{
	CvPoint center;
	int total = 0;
	int x = 0;
	int y = 0;
	for (int j = 0; j< contour.size(); j++)
	{
		total++;
		x += contour[j].x;
		y += contour[j].y;
	}
	center.x = x / ((double)total);
	center.y = y / ((double)total);
	return center;
}

void  EngineImpl::FindContours(const cv::Mat& thresholdImg,
	std::vector<std::vector<cv::Point>
	>& contours,
	int min, int max)
{
	std::vector< std::vector<cv::Point> > allContours;

	cv::findContours(thresholdImg, allContours, CV_RETR_LIST, CV_CHAIN_APPROX_NONE);
	contours.clear();
	for (size_t i = 0; i<allContours.size(); i++)
	{
		int contourSize = allContours[i].size();
		if (contourSize > min && contourSize < max)
		{
			contours.push_back(allContours[i]);
		}
	}
}
double inline __declspec (naked) __fastcall sqrt14(double n)
{
	_asm fld qword ptr[esp + 4]
		_asm fsqrt
	_asm ret 8
}

double  EngineImpl::GetDistance(double x1, double y1, double x2, double y2)
{
	double xx = (x1 - x2)*(x1 - x2);
	double yy = (y1 - y2)*(y1 - y2);
	return sqrt14(xx + yy);
}

void EngineImpl::RemovePtsNotInROI(Mat& src, CvPoint ptMass)
{
	int height = src.rows;
	int width = src.cols;
	int channels = src.channels();
	int nc = width * channels;

	for (int y = 0; y < height; y++)
	{
		uchar *data = src.ptr(y);
		for (int x = 0; x < width; x++)
		{
			if (GetDistance(x, y, ptMass.x, ptMass.y) > 660)
			{
				int xStart = x*channels;
				for (int i = 0; i< channels; i++)
					data[xStart + i] = 0;
			}
		}
	}
	return;
}


void EngineImpl::GetCircleROI(Mat& src)
{
	//Mat org = src.clone();
	Mat gray;
	cvtColor(src, gray, CV_BGR2GRAY);
	threshold(gray, gray, 30, 255, 0);
	vector<vector<cv::Point>> contours;
	int minPts = 2000;
	FindContours(gray, contours, minPts);
	if (contours.size() == 0)
	{
		return;
	}
	CvPoint ptCenter = CalcuMassCenter(contours[0]);
	RemovePtsNotInROI(src, ptCenter);
	//Mat hsl;
	//cv::cvtColor(src, hsl, CV_RGB2HLS);
	//cvtColor(src,gray,CV_BGR2GRAY);
	//adaptiveThreshold(gray,gray,255,CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 11, 5);
#if _DEBUG
	imwrite("f:\\temp\\sub\\circleROI.jpg", src);
#endif
}


vector<vector<cv::Point>> EngineImpl::MarkAllContours(Mat& src,ConstrainSettings^ constrainSettings, string filePath2Save)
{
	Mat tmp = src.clone();
	vector<vector<cv::Point>> contours;
	int minPts = constrainSettings->minSize;
	int maxPts = constrainSettings->maxSize;
	Mat gray;
	cvtColor(tmp, gray, CV_BGR2GRAY);
	//adaptiveThreshold(gray, gray, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 11, 5);
	threshold(gray, gray, 200, 255, 0);
	imwrite("f:\\temp\\sub\\adaptive.jpg", gray);
	FindContours(gray, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, i, Scalar(0, 0, 255));
	}

	imwrite(filePath2Save, tmp);
	return contours;
}

enum ChannelType
{
	blue = 0,
	red = 1,
	green = 2
};
vector<vector<cv::Point>> EngineImpl::MarkAllContoursGray(Mat& src, Mat& org)
{
	Mat gray = src.clone();
	Mat tmp = org.clone();

	vector<vector<cv::Point>> contours;
	int minPts = 10;
	int maxPts = 500;

	//adaptiveThreshold(gray, gray, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 11, 5);
	threshold(gray, gray, 200, 255, 0);
	imwrite("f:\\temp\\sub\\adaptiveYellow.jpg", gray);
	FindContours(gray, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, i, Scalar(0, 0, 255));
	}
#if _DEBUG
	imwrite("f:\\temp\\sub\\featuresYellow.jpg", tmp);
#endif
	return contours;
}


void EngineImpl::Load(string sFile)
{
	int index = sFile.rfind("\\");
	workingFolder = sFile.substr(0, index);
	img = imread(sFile);
	GetCircleROI(img);
}

string EngineImpl::MarkClones(ConstrainSettings^ constrains, std::vector<cv::Point>& centers)
{
	string resultFile = workingFolder + "\\clones.jpg";
	MarkAllContours(img, constrains,resultFile);
	return resultFile;
}