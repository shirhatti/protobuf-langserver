#include "pch.h"
#include "Generator.h"
#include <google/protobuf/descriptor.h>
#include <google/protobuf/compiler/importer.h>
#include "InMemoryInputStream.h"
using namespace google::protobuf::compiler;

using namespace google::protobuf;
using namespace google::protobuf::io;

class NoopErrorCollector : public ErrorCollector
{
public:
	NoopErrorCollector() {}
	~NoopErrorCollector() {}

	// implements ErrorCollector ---------------------------------------
	void AddError(int line, int column, const std::string& message) override {}
};

enum ErrorFormat {
	ERROR_FORMAT_GCC,  // GCC error output format (default).
	ERROR_FORMAT_MSVS  // Visual Studio output (--error_format=msvs).
};

class ErrorPrinter
	: public MultiFileErrorCollector,
	public io::ErrorCollector,
	public DescriptorPool::ErrorCollector {
public:
	ErrorPrinter(ErrorFormat format, DiskSourceTree* tree = NULL)
		: format_(format), tree_(tree), found_errors_(false) {}
	~ErrorPrinter() {}

	// implements MultiFileErrorCollector ------------------------------
	void AddError(const std::string& filename, int line, int column,
		const std::string& message) {
		found_errors_ = true;
		AddErrorOrWarning(filename, line, column, message, "error", std::cerr);
	}

	void AddWarning(const std::string& filename, int line, int column,
		const std::string& message) {
		AddErrorOrWarning(filename, line, column, message, "warning", std::clog);
	}

	// implements io::ErrorCollector -----------------------------------
	void AddError(int line, int column, const std::string& message) {
		AddError("input", line, column, message);
	}

	void AddWarning(int line, int column, const std::string& message) {
		AddErrorOrWarning("input", line, column, message, "warning", std::clog);
	}

	// implements DescriptorPool::ErrorCollector-------------------------
	void AddError(const std::string& filename, const std::string& element_name,
		const Message* descriptor, ErrorLocation location,
		const std::string& message) {
		AddErrorOrWarning(filename, -1, -1, message, "error", std::cerr);
	}

	void AddWarning(const std::string& filename, const std::string& element_name,
		const Message* descriptor, ErrorLocation location,
		const std::string& message) {
		AddErrorOrWarning(filename, -1, -1, message, "warning", std::clog);
	}

	bool FoundErrors() const { return found_errors_; }

private:
	void AddErrorOrWarning(const std::string& filename, int line, int column,
		const std::string& message, const std::string& type,
		std::ostream& out) {
		// Print full path when running under MSVS
		std::string dfile;
		if (format_ == ERROR_FORMAT_MSVS && tree_ != NULL &&
			tree_->VirtualFileToDiskFile(filename, &dfile)) {
			out << dfile;
		}
		else {
			out << filename;
		}

		// Users typically expect 1-based line/column numbers, so we add 1
		// to each here.
		if (line != -1) {
			// Allow for both GCC- and Visual-Studio-compatible output.
			switch (format_) {
			case ERROR_FORMAT_GCC:
				out << ":" << (line + 1) << ":" << (column + 1);
				break;
			case ERROR_FORMAT_MSVS:
				out << "(" << (line + 1) << ") : " << type
					<< " in column=" << (column + 1);
				break;
			}
		}

		if (type == "warning") {
			out << ": warning: " << message << std::endl;
		}
		else {
			out << ": " << message << std::endl;
		}
	}

	const ErrorFormat format_;
	DiskSourceTree* tree_;
	bool found_errors_;
};


bool generate(void* file, int64 fileSize, void* fileDescriptor)
{
	std::unique_ptr<DescriptorPool> descriptor_pool;
	InMemoryInputStream* input = new InMemoryInputStream(file, fileSize);
	std::unique_ptr<io::ZeroCopyInputStream> stream(input);

	stream->Skip(10);

	return true;
	//descriptor_pool = 

	//std::vector<const FileDescriptor*> parsed_files;
	//std::unique_ptr<DiskSourceTree> disk_source_tree;
	//std::unique_ptr<ErrorPrinter> error_collector;
	//std::unique_ptr<DescriptorPool> descriptor_pool;
	//std::unique_ptr<SimpleDescriptorDatabase> descriptor_set_in_database;
	//std::unique_ptr<SourceTreeDescriptorDatabase> source_tree_database;

	//std::vector<std::string> input_files;

	//descriptor_set_in_database.reset(new SimpleDescriptorDatabase());
	//disk_source_tree.reset(new DiskSourceTree());
	//error_collector.reset(
	//	new ErrorPrinter(ERROR_FORMAT_MSVS, disk_source_tree.get()));
	//source_tree_database.reset(new SourceTreeDescriptorDatabase(
	//	disk_source_tree.get(), descriptor_set_in_database.get()));

	//for (const auto& input_file : input_files) {

	//}

	//return false;
	//NoopErrorCollector noop_error_collector();
	//SourceTree* source_tree_;
	//FileDescriptorProto file_proto;
	//FileDescriptorProto* file_proto_ptr = &file_proto;

	//// Set up the tokenizer and parser.
	//std::unique_ptr<io::ZeroCopyInputStream> input(source_tree_->Open("greet.proto"));
	//Tokenizer tokenizer(input.get(), (ErrorCollector*)(&noop_error_collector));
	//Parser parser;

	//// Parse it.
	//file_proto_ptr->set_name("greet.proto");
	//bool parse_result = parser.Parse(&tokenizer, file_proto_ptr);
	//return parse_result;
};
