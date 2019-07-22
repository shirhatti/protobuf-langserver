echo ""
echo "----------- Restoring and compiling vscode-extension -----------"
echo ""
pushd vscode-extension

if (Test-Path node_modules)
{
    rm -r -force node_modules
}

if (Test-Path package.lock.json)
{
    rm package.lock.json
}

npm install
npm run compile

if (! $?)
{
    popd
    echo ""
    echo "vscode-extension build failed!"
    exit 1
}
popd

echo ""
echo "----------- Building language-server -----------"
echo ""
pushd language-server
dotnet build

if (! $?)
{
    popd
    echo ""
    echo "language server build failed!"
    exit 1
}
popd