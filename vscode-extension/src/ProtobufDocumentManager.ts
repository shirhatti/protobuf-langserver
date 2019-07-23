import * as vscode from 'vscode';
import { createDocument } from './ProtoDocumentFactory';
import { ProtoLogger } from './ProtoLogger';
import { getUriPath } from './UriPaths';
import { ProtoLanguageServerClient } from './ProtoLanguageServerClient';
import { ProtobufLanguage } from './ProtobufLanguage';
import { IProtobufDocument } from './IProtobufDocument';
import { IProtoDocumentChangeEvent } from './IProtoDocumentChangeEvent';
import { ProtoDocumentChangeKind } from './ProtoDocumentChangeKind';
import { AddDocumentRequest } from './RPC/AddDocumentRequest';
import { RemoveDocumentRequest } from './RPC/RemoveDocumentRequest';
import { ProtobufTextDocumentItem } from './RPC/ProtobufTextDocumentItem';

export class ProtobufDocumentManager {
    private readonly protobufDocuments: { [hostDocumentPath: string]: IProtobufDocument } = {};
    private onChangeEmitter = new vscode.EventEmitter<IProtoDocumentChangeEvent>();

    constructor(
        private readonly serverClient: ProtoLanguageServerClient,
        private readonly logger: ProtoLogger) {
    }

    public get onChange() { return this.onChangeEmitter.event; }

    public get documents() {
        return Object.values(this.protobufDocuments);
    }

    public async getDocument(uri: vscode.Uri) {
        return this._getDocument(uri);
    }

    public async getAllDocuments() {
        const files = await vscode.workspace.findFiles(ProtobufLanguage.globbingPattern);

        var results = [];
        for(const file of files) {
            try {
                const document = await vscode.workspace.openTextDocument(file);
                results.push(new ProtobufTextDocumentItem(document));  
            }
            catch{
                results.push({
                    languageId: ProtobufLanguage.id,
                    version: -1,
                    text: '',
                    uri: file.toString(),
                });
            }
        }

        return results;
    }

    public async getActiveDocument() {
        if (!vscode.window.activeTextEditor) {
            return null;
        }

        if (vscode.window.activeTextEditor.document.languageId !== ProtobufLanguage.id) {
            return null;
        }

        const activeDocument = await this.getDocument(vscode.window.activeTextEditor.document.uri);
        return activeDocument;
    }

    public async initialize() {
        const documentUris = await vscode.workspace.findFiles(ProtobufLanguage.globbingPattern);

        for (const uri of documentUris) {
            // Add the document but don't notify the server
            this.addDocument(uri, false);
        }

        for (const textDocument of vscode.workspace.textDocuments) {
            if (textDocument.languageId !== ProtobufLanguage.id)
            {
                continue;
            }

            this.openDocument(textDocument.uri);
        }
    }

    public register() {
        // Track future documents
        const watcher = vscode.workspace.createFileSystemWatcher(ProtobufLanguage.globbingPattern);
        const didCreateRegistration = watcher.onDidCreate(
            async (uri: vscode.Uri) => this.addDocument(uri, true));
        const didDeleteRegistration = watcher.onDidDelete(
            async (uri: vscode.Uri) => this.removeDocument(uri));
        const didOpenRegistration = vscode.workspace.onDidOpenTextDocument(document => {
            if (document.languageId !== ProtobufLanguage.id) {
                return;
            }

            this.openDocument(document.uri);
        });
        const didCloseRegistration = vscode.workspace.onDidCloseTextDocument(document => {
            if (document.languageId !== ProtobufLanguage.id) {
                return;
            }

            this.closeDocument(document.uri);
        });
        const didChangeRegistration = vscode.workspace.onDidChangeTextDocument(async args => {
            if (args.document.languageId !== ProtobufLanguage.id) {
                return;
            }

            this.documentChanged(args.document.uri);
        });

        return vscode.Disposable.from(
            watcher,
            didCreateRegistration,
            didDeleteRegistration,
            didOpenRegistration,
            didCloseRegistration,
            didChangeRegistration);
    }

    private _getDocument(uri: vscode.Uri) {
        const path = getUriPath(uri);
        const document = this.protobufDocuments[path];

        if (!document) {
            throw new Error('Requested document does not exist.');
        }

        return document;
    }

    private openDocument(uri: vscode.Uri) {
        const document = this._getDocument(uri);

        this.notifyDocumentChange(document, ProtoDocumentChangeKind.opened);
    }

    private closeDocument(uri: vscode.Uri) {
        const document = this._getDocument(uri);

        // VSCode resets all sync versions when a document closes.
        document.reset();

        this.notifyDocumentChange(document, ProtoDocumentChangeKind.closed);
    }

    private async documentChanged(uri: vscode.Uri) {
        const document = await this._getDocument(uri);

        const activeTextEditor = vscode.window.activeTextEditor;
        if (activeTextEditor && activeTextEditor.document.uri === uri) {
            this.notifyDocumentChange(document, ProtoDocumentChangeKind.changed);
        }
    }

    private addDocument(uri: vscode.Uri, notifyServer: boolean = true) {
        const document = createDocument(uri);
        this.protobufDocuments[document.path] = document;

        const request = new AddDocumentRequest(document.uri.fsPath);
        if(notifyServer)
        {
            this.serverClient.sendRequest<AddDocumentRequest>('proto/addDocument', request);
        }
        
        this.notifyDocumentChange(document, ProtoDocumentChangeKind.added);

        return document;
    }

    private removeDocument(uri: vscode.Uri) {
        const document = this._getDocument(uri);
        delete this.protobufDocuments[document.path];

        const request = new RemoveDocumentRequest(document.uri.fsPath);
        this.serverClient.sendRequest<RemoveDocumentRequest>('proto/removeDocument', request);

        this.notifyDocumentChange(document, ProtoDocumentChangeKind.removed);
    }

    private notifyDocumentChange(document: IProtobufDocument, kind: ProtoDocumentChangeKind) {
        if (this.logger.verboseEnabled) {
            this.logger.logVerbose(
                `Notifying document '${getUriPath(document.uri)}' changed '${ProtoDocumentChangeKind[kind]}'`);
        }

        const args: IProtoDocumentChangeEvent = {
            document,
            kind,
        };

        this.onChangeEmitter.fire(args);
    }
}