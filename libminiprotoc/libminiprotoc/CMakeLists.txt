# CMakeList.txt : CMake project for libminiprotoc, include source and define
# project specific logic here.
#
cmake_minimum_required (VERSION 3.8)

set(libminiprotoc_files
  "inmemoryinputstream.cpp"
  "libminiprotoc.cpp"
)

set(libminiprotoc_includes
  "inmemoryinputstream.h"
  "libminiprotoc.h"
)

# Add source to this project's executable.
add_library (libminiprotoc SHARED ${libminiprotoc_files} ${libminiprotoc_includes})

# TODO: Add tests and install targets if needed.
find_package(protobuf REQUIRED)
target_link_libraries(libminiprotoc PRIVATE protobuf::libprotoc protobuf::libprotobuf protobuf::libprotobuf-lite)