#pragma once
#include <google/protobuf/io/zero_copy_stream.h>
#include <google/protobuf/stubs/port.h>

class InMemoryInputStream :
 public google::protobuf::io::ZeroCopyInputStream
{
public:
	InMemoryInputStream(void* buffer, google::protobuf::int64 size);
	~InMemoryInputStream();
	bool Next(const void** data, int* size) override;
	void BackUp(int count) override;
	bool Skip(int count) override;
	google::protobuf::int64 ByteCount() const override;

private:
	void* data;
	google::protobuf::int64 size;
	void* currentPosition;

};

