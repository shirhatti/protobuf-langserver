/* --------------------------------------------------------------------------------------------
 * Copyright (c) Microsoft Corporation. All rights reserved.
 * Licensed under the MIT License. See License.txt in the project root for license information.
 * ------------------------------------------------------------------------------------------ */

import * as fs from 'fs';
import * as path from 'path';
import * as vscode from 'vscode';
import { Trace } from 'vscode-languageclient';

export class ProtoLogger implements vscode.Disposable {
    public static readonly logName = 'Proto Log';
    public readonly verboseEnabled: boolean;
    public readonly messageEnabled: boolean;
    public readonly outputChannel: vscode.OutputChannel;

    constructor(public readonly trace: Trace) {
        this.verboseEnabled = this.trace >= Trace.Verbose;
        this.messageEnabled = this.trace >= Trace.Messages;

        this.outputChannel = vscode.window.createOutputChannel(ProtoLogger.logName);

        this.logRazorInformation();
    }

    public logAlways(message: string) {
        this.logWithmarker(message);
    }

    public logError(message: string, error: Error) {
        // Always log errors
        const errorPrefixedMessage = `(Error) ${message}
${error.message}
Stack Trace:
${error.stack}`;
        this.logAlways(errorPrefixedMessage);
    }

    public logMessage(message: string) {
        if (this.messageEnabled) {
            this.logWithmarker(message);
        }
    }

    public logVerbose(message: string) {
        if (this.verboseEnabled) {
            this.logWithmarker(message);
        }
    }

    public dispose() {
        this.outputChannel.dispose();
    }

    private logWithmarker(message: string) {
        const timeString = new Date().toLocaleTimeString();
        const markedMessage = `[Client - ${timeString}] ${message}`;

        this.log(markedMessage);
    }

    private log(message: string) {
        this.outputChannel.appendLine(message);
    }

    private logRazorInformation() {
        const packageJsonContents = readOwnPackageJson();

        this.log(
            '--------------------------------------------------------------------------------');
        this.log(`Protobufflab VSCode version ${packageJsonContents.version}`);
        this.log(
            '--------------------------------------------------------------------------------');
        this.log('');
    }
}

function readOwnPackageJson() {
    const packageJsonPath = findInDirectoryOrAncestor(__dirname, 'package.json');
    return JSON.parse(fs.readFileSync(packageJsonPath).toString());
}

function findInDirectoryOrAncestor(dir: string, filename: string) {
    while (true) {
        const candidate = path.join(dir, filename);
        if (fs.existsSync(candidate)) {
            return candidate;
        }

        const parentDir = path.dirname(dir);
        if (parentDir === dir) {
            throw new Error(`Could not find '${filename}' in or above '${dir}'.`);
        }

        dir = parentDir;
    }
}