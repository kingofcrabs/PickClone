#pragma once
#include "EngineImpl.h"


namespace EngineDll
{
	public ref class MPoint
	{
	public:
		int x;
		int y;
		MPoint(int xx, int yy)
		{
			x = xx;
			y = yy;
		}
	};
	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		void Load(System::String^ sFile);
		System::String^ MarkClones(ConstrainSettings^ ConstrainSettings, int% actualCnt, array<MPoint^>^ %features);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

