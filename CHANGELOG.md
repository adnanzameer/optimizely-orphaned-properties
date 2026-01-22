# Changelog

## Changes in version 2.0.4

### Added
- Added a UI toggle to show or hide Optimizely Forms properties in the Orphaned Properties admin view.
- Introduced automatic categorization of all Optimizely Forms related properties under a new `Form` category.
- Added support for detecting inherited and custom Form Container blocks without requiring the Optimizely Forms package to be installed.
- Enabled query string based state management (`showFormProperties`) with automatic pagination reset when toggled.

### Changed
- Refactored orphan detection logic to keep model validation and UI filtering concerns separate.
- Improved Forms property detection using content type signatures and system property markers rather than direct type references.
- Updated results sorting to group and display FORM category entries consistently.

## Changes in version 2.0.3
- Fixed an exception thrown when retrieving contentType.ModelTypeString.
- Improved the logic used when deleting properties.
- General code quality improvements.
- Added table column sorting support.

## Changes in version 2.0.2
- Added anti-forgery token and footer client resources to fix failed Optimizely AJAX calls in custom plugin pages. (Thanks to @huerpfse)

## Changes in version 2.0.1
- Revert the framework support back to .NET Core 6 only.

- ## Changes in version 2.0.0
- Updated for .NET Core 7, 8, and 9 support.
- Refined the logic to locate orphaned properties
 
## Changes in version 1.5.0
- Bug fix for missing resource file.
- Bug fix for missing settings menu item 

## Changes in version 1.0.0
- Released new nuget package: Plugin to identify and bulk delete orphaned or missing properties (missing from code) in Optimizely CMS 12.