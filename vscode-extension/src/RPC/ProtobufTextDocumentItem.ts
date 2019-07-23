import * as vscode from 'vscode';

export class ProtobufTextDocumentItem {
    public readonly languageid: string;
    public readonly version: number;
    public readonly text: string;
    public readonly uri: string;

    constructor(document: vscode.TextDocument){
        this.languageid = document.languageId;
        this.version = document.version;
        this.text = document.getText();
        this.uri = document.uri.toString();
    }
}