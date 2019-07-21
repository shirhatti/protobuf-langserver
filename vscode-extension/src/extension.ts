import * as vscode from 'vscode';
import { ProtoLogger } from './ProtoLogger';
import { Trace } from 'vscode-languageclient';
import { ProtoLanguageServerClient } from './ProtoLanguageServerClient';
import { resolveLanguageServerPath } from './ResolveLanguageServerPath';

export async function activate(context: vscode.ExtensionContext) {
	const verbosity = Trace.Messages;
	const logger = new ProtoLogger(verbosity);
	const languageServerPath = resolveLanguageServerPath();
	const protoConfigSection = vscode.workspace.getConfiguration('proto');
	const debugLanguageServer = protoConfigSection.get<boolean>('debug');
	const debug = debugLanguageServer ? debugLanguageServer : false;
	const languageServerClient = new ProtoLanguageServerClient(languageServerPath, debug, logger);

	languageServerClient.onStart(() => {
		// Once the language server is started this will be called, it's where we can register all of our
		// bits with VSCode for them to function properly.
	});

	await languageServerClient.start();

	context.subscriptions.push(languageServerClient);
}

// this method is called when your extension is deactivated
export function deactivate() {}