#pragma once

#ifdef LIBMINIPROTOC_EXPORTS
#define GENERATOR_API __declspec(dllexport)
#else
#define GENERATOR_API __declspec(dllimport)
#endif

#include <google/protobuf/stubs/port.h>

extern "C" GENERATOR_API bool generate(void*, google::protobuf::int64, void*, google::protobuf::int64&);
