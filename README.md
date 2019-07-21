# protobuf-langserver

```cmd
msbuild protobuf.sln

.\Debug\protoc.exe --plugin=protoc-gen-cs="src/protoc-gen-cs/bin/Debug/netcoreapp3.0/protoc-gen-cs.exe" --cs_out=. --proto_path=<proto_path> <proto_path>\addressbook.proto
```
