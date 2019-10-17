import { Record } from '@orbit/data';
import { AttributesObject, ResourceObject } from 'jsonapi-typescript';

export type PROJECT_IMPORTS_TYPE = 'project-imports';
export const TYPE_NAME = 'project-import';
export const PLURAL_NAME = 'project-imports';

export interface ProjectImportAttributes extends AttributesObject {
  importData?: string;
  dateCreated?: string;
  dateUpdated?: string;
}

export type ProjectImportResource = ResourceObject<PROJECT_IMPORTS_TYPE, ProjectImportAttributes> &
  Record;
