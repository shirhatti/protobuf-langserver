import { IProtobufDocument } from './IProtobufDocument';
import { getUriPath } from './UriPaths';
import * as vscode from 'vscode';

export class ProtobufDocument implements IProtobufDocument {
    public readonly path: string;
    private content = '';
    private hostDocumentVersion: number | null = null;

    public constructor(public readonly uri: vscode.Uri) {
        this.path = getUriPath(uri);
    }

    public get hostDocumentSyncVersion(): number | null {
        return this.hostDocumentVersion;
    }

    public getContent() {
        return this.content;
    }

    public setContent(content: string, hostDocumentVersion: number) {
        this.hostDocumentVersion = hostDocumentVersion;
        this.content = content;
    }

    public reset() {
        this.hostDocumentVersion = null;
        this.content = '';
    }
}