# FlatCopy Profile Exporter

FlatCopy Profile Exporter is a Windows 10/11 desktop utility for copying user-profile data to another folder with no compression. It is a straight file-and-folder copy tool with a graphical interface, progress bar, cancel button, and plain-text logging.

## Features

- Native Windows GUI built with WinForms
- Select one or more local user profiles
- Copy either:
  - selected standard profile folders
  - the entire profile
- Flat file copy only
- Progress tracking during long copy jobs
- Plain-text log written to the destination folder
- Optional destination-drive BitLocker requirement
- Optional Windows EFS encryption for copied output
- Skip reparse-point directories to avoid recursion problems

## Supported Folder Mode

When folder mode is used, the app can copy:

- Desktop
- Documents
- Downloads
- Pictures
- Music
- Videos
- Favorites
- Contacts
- Links
- Saved Games
- Searches

## Whole Profile Mode

If **Copy entire profile** is enabled, the app copies the full user profile tree into the destination and ignores the folder checklist.

Output layout:

```text
<destination>\
  <profile-name>\
    ...
```

## Destination Layout

Standard folder mode creates output like this:

```text
<destination>\
  <profile-name>\
    Desktop\
    Documents\
    Downloads\
    Pictures\
    ...
```

## Logging

Each run creates a plain-text log file in the selected destination folder:

```text
FlatCopyLog_yyyyMMdd_HHmmss.txt
```

The log includes:

- start time
- selected profiles
- selected mode
- missing folders
- skipped files
- copy failures
- final totals

## Optional Security Controls

The app includes two optional Windows security controls:

- `Require BitLocker-protected destination`
  - Verifies that the destination volume reports BitLocker protection before the copy begins.
- `Encrypt copied data with Windows EFS`
  - Applies Windows Encrypting File System (EFS) encryption to the copied output folders and the generated log file after the copy completes.

Notes:

- EFS requires an `NTFS` destination volume.
- BitLocker verification depends on Windows being able to report the destination volume status.
- These options use built-in Windows security features. They are intentionally labeled as `BitLocker` and `EFS` rather than as a formal certification claim.

## Build

This project targets `.NET 8` Windows desktop.

```powershell
dotnet build FlatCopyProfileExporter.csproj
```

## Publish

```powershell
dotnet publish FlatCopyProfileExporter.csproj -c Release -r win-x64 --self-contained false
```

## Run

After publishing, launch:

```text
bin\Release\net8.0-windows\win-x64\publish\FlatCopyProfileExporter.exe
```

## Notes

- The app does not compress, archive, or package files.
- The destination folder cannot be inside a selected source profile.
- Some locked or protected files may still fail to copy depending on permissions.
- EFS encryption may not be available on all Windows editions or destination filesystems.

## License

This project uses a custom license that allows personal use, internal commercial use,
modification, and free redistribution with attribution, but does not allow selling the
software itself. See [LICENSE.txt](D:/dev/flatcopy/LICENSE.txt).
