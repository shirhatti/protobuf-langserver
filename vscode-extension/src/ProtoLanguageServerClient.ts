/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import * as vscode from 'vscode';
import {
    GenericRequestHandler,
    LanguageClient,
    LanguageClientOptions,
    ServerOptions,
    State,
    Trace,
} from 'vscode-languageclient/lib/main';
import { ProtoLogger } from './ProtoLogger';
import { EventEmitter } from 'events';
import { listeners } from 'cluster';

const events = {
    ServerStart: 'ServerStart',
    ServerStop: 'ServerStop',
};

export class ProtoLanguageServerClient implements vscode.Disposable {
    private clientOptions: LanguageClientOptions;
    private serverOptions: ServerOptions;
    private client: LanguageClient;
    private startDisposable: vscode.Disposable | undefined;
    private onStartedListeners: Array<() => Promise<any>> = [];
    private isStarted: boolean;
    private startHandle: Promise<void> | undefined;
    private eventBus: EventEmitter;

    constructor(
        serverPath: string,
        debug: boolean,
        private readonly logger: ProtoLogger) {
        this.eventBus = new EventEmitter();
        this.isStarted = false;
        this.clientOptions = {
            outputChannel: logger.outputChannel,
        };

        const args: string[] = [];
        let command = serverPath;
        if (serverPath.endsWith('.dll')) {
            this.logger.logMessage('Proto Language Server path is an assembly. ' +
                'Using \'dotnet\' from the current path to start the server.');

            command = 'dotnet';
            args.push(serverPath);
        }

        this.logger.logMessage(`Proto language server path: ${serverPath}`);

        args.push('-lsp');
        args.push('--logLevel');
        const logLevelString = this.getLogLevelString(logger.trace);

        args.push(logLevelString);

        if (debug) {
            this.logger.logMessage('Debug flag set for Proto Language Server.');
            args.push('--debug');
        }

        this.serverOptions = {
            run: { command, args },
            debug: { command, args },
        };

        this.client = new LanguageClient(
            'protoLanguageServer', 'Proto Language Server', this.serverOptions, this.clientOptions);

        this.eventBus = new EventEmitter();
    }

    public onStarted(listener: () => Promise<any>) {
        this.onStartedListeners.push(listener);
    }

    public onStart(listener: () => any) {
        this.eventBus.addListener(events.ServerStart, listener);

        const disposable = new vscode.Disposable(() =>
            this.eventBus.removeListener(events.ServerStart, listener));
        return disposable;
    }

    public onStop(listener: () => any) {
        this.eventBus.addListener(events.ServerStop, listener);

        const disposable = new vscode.Disposable(() =>
            this.eventBus.removeListener(events.ServerStop, listener));
        return disposable;
    }

    public async start() {
        if (this.startHandle) {
            return this.startHandle;
        }

        let resolve: () => void = Function;
        let reject: (reason: any) => void = Function;
        this.startHandle = new Promise<void>((resolver, rejecter) => {
            resolve = resolver;
            reject = rejecter;
        });

        try {
            this.logger.logMessage('Starting Proto Language Server...');
            const startDisposable = this.client.start();
            this.startDisposable = startDisposable;
            this.logger.logMessage('Server started, waiting for client to be ready...');
            this.client.onReady().then(async () => {
                this.isStarted = true;
                this.logger.logMessage('Server started and ready!');
                this.eventBus.emit(events.ServerStart);

                for(const listener of this.onStartedListeners) {
                    await listener();
                }

                // Succesfully started, notify listeners.
                resolve();
            });
        } catch (error) {
            vscode.window.showErrorMessage(
                'Proto Language Server failed to start unexpectedly, ' +
                'please check the \'Proto Log\' and report an issue.');

            reject(error);
        }

        return this.startHandle;
    }

    public async sendRequest<TResponseType>(method: string, param: any) {
        if (!this.isStarted) {
            throw new Error('Tried to send requests while server is not started.');
        }

        return this.client.sendRequest<TResponseType>(method, param);
    }

    public async onRequest<TRequest, TReturn>(method: string, handler: GenericRequestHandler<TRequest, TReturn>) {
        if (!this.isStarted) {
            throw new Error('Tried to bind on request logic while server is not started.');
        }

        this.client.onRequest(method, handler);
    }

    public dispose() {
        this.logger.logMessage('Stopping Proto Language Server.');

        if (this.startDisposable) {
            this.startDisposable.dispose();
        }

        this.isStarted = false;
        this.startHandle = undefined;
        this.eventBus.emit(events.ServerStop);
    }

    private getLogLevelString(trace: Trace) {
        switch (trace) {
            case Trace.Off:
                return 'None';
            case Trace.Messages:
                return 'Information';
            case Trace.Verbose:
                return 'Trace';
        }

        throw new Error(`Unexpected trace value: '${Trace[trace]}'`);
    }
}