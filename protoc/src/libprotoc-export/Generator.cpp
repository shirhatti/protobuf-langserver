#include "pch.h"
#include "Generator.h"
#include <google/protobuf/descriptor.h>
#include <google/protobuf/compiler/importer.h>
#include "InMemoryInputStream.h"
#include <google/protobuf/io/io_win32.h>
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

bool generate(void* file, int64 fileSize, void* fileDescriptor)
{
	const std::string& filename = "greet.proto";
	std::unique_ptr<DescriptorPool> descriptor_pool;
	InMemoryInputStream* inputStream = new InMemoryInputStream(file, fileSize);
	std::unique_ptr<io::ZeroCopyInputStream> input(inputStream);
	SingleFileErrorCollector file_error_collector(filename, NULL);
	io::Tokenizer tokenizer(input.get(), &file_error_collector);

	Parser parser;
	FileDescriptorProto* output = new(fileDescriptor) FileDescriptorProto();
	return parser.Parse(&tokenizer, output);

	//std::unique_ptr<DiskSourceTree> disk_source_tree;
	//std::unique_ptr<ErrorPrinter> error_collector;
	//std::unique_ptr<DescriptorPool> descriptor_pool;
	//std::unique_ptr<SimpleDescriptorDatabase> descriptor_set_in_database;
	//std::unique_ptr<SourceTreeDescriptorDatabase> source_tree_database;
	//
	//// Initialize descriptor set to empty. In future we may be passed in a descriptor set
	//descriptor_set_in_database.reset(new SimpleDescriptorDatabase());

	//disk_source_tree.reset(new DiskSourceTree());

	//// Initialize disk source tree
	//// TODO AddDefaultProtoPaths. See command_line_interface.c:227


	//error_collector.reset(
	//	new ErrorPrinter(ERROR_FORMAT_MSVS, disk_source_tree.get()));


	//source_tree_database.reset(new SourceTreeDescriptorDatabase(
	//	disk_source_tree.get(), descriptor_set_in_database.get()));
	//source_tree_database->RecordErrorsTo(error_collector.get());

	//descriptor_pool.reset(new DescriptorPool(
	//	source_tree_database.get(),
	//	source_tree_database->GetValidationErrorCollector()));

	//descriptor_pool->EnforceWeakDependencies(true);
	
	// TODO: initialize descriptor pool

	return true;

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
