#pragma once
#include "EngineImpl.h"

namespace EngineDll
{
	public ref class IEngine
	{
	public:
		IEngine();
		~IEngine();
		void Load(System::String^ sFile);
		System::String^ MarkClones(CloneConstrain^ cloneConstrain, array<System::Drawing::Point^>^ %features);
	private :
		EngineImpl* m_EngineImpl;
	};



	
	

}

