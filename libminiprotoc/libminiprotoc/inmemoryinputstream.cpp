#include "inmemoryinputstream.h"

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
	// Handle seeking. This only works for first read
	if (this->currentPosition == 0)
	{
		*data = this->data;
		*size = this->size;
		this->currentPosition = this->size;
		return true;
	}
	return false;
}

void InMemoryInputStream::BackUp(int count) {
	currentPosition -= count;
}

bool InMemoryInputStream::Skip(int count) {
	if (currentPosition + count > size)
	{
		return false;
	}
	currentPosition += count;
	return true;
}

int64 InMemoryInputStream::ByteCount() const {
	return size;
}
