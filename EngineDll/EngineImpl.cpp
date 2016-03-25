#include "stdafx.h"
#include "EngineImpl.h"

using namespace std;
using namespace cv;

EngineImpl::EngineImpl()
{

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

bool CompareX(Point x,Point y) { return x.x<y.x; }

int EngineImpl::GetWidth(vector<Point> pts)
{
	int right = max_element(pts.begin(), pts.end(), CompareX)->x;
	int left = min_element(pts.begin(), pts.end(), CompareX)->x;
	return right - left;
}

void EngineImpl::GetCircleROI(Mat& src)
{
	Mat gray;
	cvtColor(src, gray, CV_BGR2GRAY);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite("h:\\temp\\output\\gray.jpg", gray);
#endif
	vector<vector<cv::Point>> contours;
	int minPts = 1000;
	FindContours(gray, contours, minPts);
	if (contours.size() == 0)
	{
		return;
	}
	int max = 0;
	int index = 0;
	for (int i = 0; i< contours.size(); i++)
	{
		int width = GetWidth(contours[i]);
		if (width > max)
		{
			max = width;
			index = i;
		}
	}
	edgeContour = contours[index];
#if _DEBUG	
	Mat tmp = src.clone();

	//for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, index, Scalar(0, 255, 0), 2);
	}
	imwrite("h:\\temp\\output\\circleROI.jpg", tmp);
#endif
}


vector<vector<cv::Point>> EngineImpl::MarkAllContours(Mat& src,ConstrainSettings^ constrainSettings, string filePath2Save)
{
	Mat tmp = src.clone();
	vector<vector<cv::Point>> contours;
	int minPts = constrainSettings->minSize;
	int maxPts = constrainSettings->maxSize;
	Mat gray;
	auto pt = GetMassCenter(edgeContour);
	RemovePtsNotInROI(tmp, pt);
	cvtColor(tmp, gray, CV_BGR2GRAY);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite("h:\\temp\\output\\threshold.jpg", gray);
#endif
	FindContours(gray, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, i, Scalar(0, 0, 255));
	}
	imwrite(filePath2Save, tmp);
	return contours;
}


string EngineImpl::MarkClones(ConstrainSettings^ constrains, std::vector<cv::Point>& centers)
{
	string resultFile = workingFolder + "\\output\\clones.jpg";
	auto contours = MarkAllContours(img, constrains, resultFile);
	sort(contours.begin(), contours.end(), [](const vector<cv::Point> & a, const vector<cv::Point> & b) -> bool
	{
		return a.size() > b.size();
	});

	for (auto contour : contours)
	{
		cv::Point pt = GetMassCenter(contour);
		centers.push_back(pt);
	}
	return resultFile;
}

enum ChannelType
{
	blue = 0,
	red = 1,
	green = 2
};

cv::Point EngineImpl::GetMassCenter(vector<cv::Point>& pts)
{
	int size = pts.size();
	float totalX = 0.0, totalY = 0.0;
	for (int i = 0; i<size; i++) {
		totalX += pts[i].x;
		totalY += pts[i].y;
	}
	return cv::Point(totalX / size, totalY / size); // condition: size != 0
}


//first convert the image into HSV format,call inRange to filter the color
vector<vector<cv::Point>> EngineImpl::MarkAllContoursGray(Mat& src, Mat& org)
{
	Mat gray = src.clone();
	Mat tmp = org.clone();

	vector<vector<cv::Point>> contours;
	int minPts = 10;
	int maxPts = 500;

	//adaptiveThreshold(gray, gray, 255, CV_ADAPTIVE_THRESH_MEAN_C, CV_THRESH_BINARY_INV, 11, 5);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite("h:\\temp\\output\\adaptiveYellow.jpg", gray);
#endif
	FindContours(gray, contours, minPts, maxPts);
	for (int i = 0; i< contours.size(); i++)
	{
		drawContours(tmp, contours, i, Scalar(0, 0, 255),1);
		
	}
#if _DEBUG
	imwrite("h:\\temp\\output\\featuresYellow.jpg", tmp);
#endif
	return contours;
}


void EngineImpl::Load(string sFile)
{
	int index = sFile.rfind("\\Data");
	workingFolder = sFile.substr(0, index);
	img = imread(sFile);
	GetCircleROI(img);
}

void EngineImpl::FindRefPositions(int& top, int& left, int& bottom, int& right)
{
	Mat gray;
	Mat src = img.clone();
	//cvtColor(img, gray, CV_BGR2GRAY);
#if _DEBUG
	imwrite("h:\\temp\\output\\beforeRemovePetriDish.jpg", src);
#endif
	//fill Petri dishe big contour with black
	vector < vector<Point>> tmpContours;
	tmpContours.push_back(edgeContour);
	drawContours(src, tmpContours,0, Scalar(0, 0, 0), 5);
	drawContours(src, tmpContours, 0, Scalar(0, 0, 0), CV_FILLED);
	cvtColor(src, gray, CV_BGR2GRAY);
	threshold(gray, gray, 200, 255, 0);
#if _DEBUG
	imwrite("h:\\temp\\output\\removePetriDish.jpg", gray);
#endif
	vector<vector<cv::Point>> contours;
	int minPts = 10;
	int maxPts = 200;
	FindContours(gray, contours, minPts, maxPts);
	vector<Point> pts;
	for (auto contour : contours)
	{
		pts.push_back(GetMassCenter(contour));
	}
	int tmpTop = 10000;
	int tmpLeft = 10000;
	int tmpRight = 0;
	int tmpBottom = 0;
	for (auto pt : pts)
	{
		if (pt.y < tmpTop)
			tmpTop = pt.y;
		if (pt.y > tmpBottom)
			tmpBottom = pt.y;
		if (pt.x < tmpLeft)
			tmpLeft = pt.x;
		if (pt.x > tmpRight)
			tmpRight = pt.x;
	}
	top = tmpTop;
	bottom = tmpBottom;
	left = tmpLeft;
	right = tmpRight;
}

