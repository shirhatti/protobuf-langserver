#pragma once

#ifdef LIBMINIPROTOC_EXPORTS
#define GENERATOR_API __declspec(dllexport)
#else
#define GENERATOR_API __declspec(dllimport)
#endif

extern "C" GENERATOR_API bool generate(void*, void*);
