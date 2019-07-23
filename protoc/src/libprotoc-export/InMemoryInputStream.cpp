#include "pch.h"
#include "InMemoryInputStream.h"

using namespace google::protobuf;
using namespace google::protobuf::io;

InMemoryInputStream::InMemoryInputStream(void* data, int64 size)
{
	this->data = data;
	this->size = size;
	currentPosition = 0;
};

InMemoryInputStream::~InMemoryInputStream() {};

bool InMemoryInputStream::Next(const void** data, int* size) {
	*data = currentPosition;
	return false;
}

void InMemoryInputStream::BackUp(int count) {
	currentPosition = (void*)((char*)currentPosition - count);
}

bool InMemoryInputStream::Skip(int count) {
	currentPosition = (void*)((char*)currentPosition + count);
	return false;
}

int64 InMemoryInputStream::ByteCount() const {
	return size;
}
