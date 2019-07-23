import * as vscode from 'vscode';

export class ProtobufLanguage {
    public static id = "protobuf";
    public static fileExtensions = [".proto"];
    public static globbingPattern = `**/*{${ProtobufLanguage.fileExtensions.join(',')}}`;
    public static documentSelector: vscode.DocumentSelector = { pattern: ProtobufLanguage.globbingPattern };
}