#include "IEngine.h"
#include <msclr\marshal_cppstd.h>

namespace EngineDll
{
	IEngine::IEngine()
	{
		m_EngineImpl = new EngineImpl();
	}


	IEngine::~IEngine()
	{
		delete m_EngineImpl;
	}


	void IEngine::Load(System::String^ sFile)
	{
		std::string nativeSFile = msclr::interop::marshal_as< std::string >(sFile);
		m_EngineImpl->Load(nativeSFile);
	}

	System::String^ IEngine::MarkClones(ConstrainSettings^ ConstrainSettings, int% actualCnt, array<MPoint^>^ %features)
	{

		//System::String^ MyString = gcnew System::String(Model.c_str());
		std::vector<cv::Point> pts;
		std::string resFile = m_EngineImpl->MarkClones(ConstrainSettings, pts);
		int i = 0;
		for (auto pt : pts)
		{
			MPoint^ sysPoint = gcnew MPoint(pt.x, pt.y);
			features[i++] = sysPoint;
		}
		return gcnew System::String(resFile.c_str());
	}

}
