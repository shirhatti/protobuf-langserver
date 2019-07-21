import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';

export function resolveLanguageServerPath() {
	const languageServerDir = path.join(
        __dirname, '..', '..', 'language-server', 'src', 'ProtobufLanguageServer',
        'bin', 'Debug', 'netcoreapp2.2');

    if (!fs.existsSync(languageServerDir)) {
        throw new Error('The Proto Language Server project has not yet been built - '
            + `could not find ${languageServerDir}`);
    }

    const executable = findLanguageServerExecutable(languageServerDir);
    return executable;
}

function findLanguageServerExecutable(withinDir: string) {
    const extension = isWindows() ? '.exe' : '';
    const executablePath = path.join(
        withinDir,
        `ProtobufLanguageServer${extension}`);
    let fullPath = '';

    if (fs.existsSync(executablePath)) {
        fullPath = executablePath;
    } else {
        // Exe doesn't exist.
        const dllPath = path.join(
            withinDir,
            'ProtobufLanguageServer.dll');

        if (!fs.existsSync(dllPath)) {
            throw new Error(`Could not find Proto Language Server executable within directory '${withinDir}'`);
        }

        fullPath = dllPath;
    }

    return fullPath;
}

function isWindows() {
    return !!os.platform().match(/^win/);
}