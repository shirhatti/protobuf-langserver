import { IProtobufDocument } from "./IProtobufDocument";
import { ProtoDocumentChangeKind } from "./ProtoDocumentChangeKind";

export interface IProtoDocumentChangeEvent {
    readonly document: IProtobufDocument;
    readonly kind: ProtoDocumentChangeKind;
}