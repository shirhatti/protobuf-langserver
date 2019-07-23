import * as vscode from 'vscode';
import { ProtobufDocument } from './ProtobufDocument';

export function createDocument(uri: vscode.Uri) {
    const document = new ProtobufDocument(uri);
    return document;
}