import * as vscode from 'vscode';

export function getUriPath(uri: vscode.Uri) {
    const uriPath = uri.fsPath || uri.path;
    return uriPath;
}