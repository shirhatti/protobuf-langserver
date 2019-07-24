#include <google/protobuf/descriptor.h>
#include <google/protobuf/compiler/importer.h>

#include "libminiprotoc.h"
#include "inmemoryinputstream.h"

using namespace google::protobuf::compiler;
using namespace google::protobuf;
using namespace google::protobuf::io;

class SingleFileErrorCollector
	: public io::ErrorCollector{
 public:
  SingleFileErrorCollector(const std::string & filename,
						   MultiFileErrorCollector * multi_file_error_collector)
	  : filename_(filename),
		multi_file_error_collector_(multi_file_error_collector),
		had_errors_(false) {}
  ~SingleFileErrorCollector() {}

  bool had_errors() { return had_errors_; }

  // implements ErrorCollector ---------------------------------------
  void AddError(int line, int column, const std::string & message) override {
	if (multi_file_error_collector_ != NULL) {
	  multi_file_error_collector_->AddError(filename_, line, column, message);
	}
	had_errors_ = true;
  }

 private:
  std::string filename_;
  MultiFileErrorCollector* multi_file_error_collector_;
  bool had_errors_;
};

bool generate(void* file, int64 fileSize, void* fileDescriptor, int64& fileDescriptorSize)
{
	const std::string& filename = "greet.proto";
	std::unique_ptr<DescriptorPool> descriptor_pool;
	InMemoryInputStream* inputStream = new InMemoryInputStream(file, fileSize);
	std::unique_ptr<io::ZeroCopyInputStream> input(inputStream);
	SingleFileErrorCollector file_error_collector(filename, NULL);
	io::Tokenizer tokenizer(input.get(), &file_error_collector);

	Parser parser;
	FileDescriptorProto* output = new FileDescriptorProto();
	bool parseResult = parser.Parse(&tokenizer, output);
	fileDescriptorSize = output->ByteSizeLong();
	output->SerializeToArray(fileDescriptor, fileDescriptorSize);
	delete(output);
	return parseResult;
};
