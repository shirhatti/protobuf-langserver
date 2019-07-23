import * as vscode from 'vscode';

export interface IProtobufDocument {
    readonly path: string;
    readonly uri: vscode.Uri;
    getContent(): string;
    reset(): void;
}