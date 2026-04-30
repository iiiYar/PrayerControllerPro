# Release History

Git tags are used for source checkpoints. Build output folders under `releases\` are local artifacts and are ignored by Git.

## Source Tags

| Version | Source state | Notes |
| --- | --- | --- |
| `v1.0.2` | Legacy WinForms checkpoint | Last legacy source checkpoint available in Git history. |
| `v1.0.10` | Current WPF/.NET 8 checkpoint | Clean WPF project structure with Volume Guard. |
| `v1.0.11` | Repository cleanup checkpoint | Removed legacy files, removed legacy settings migration, added handoff docs. |
| `v1.1.0` | District timing and smooth Volume Guard checkpoint | Added optional Riyadh/Jeddah districts via coordinates and stepped volume fade styles. |
| `v1.1.1` | Local dashboard polish checkpoint | Refined the next-prayer dashboard card layout. |

## Local Build Artifacts

The local workspace may contain generated folders such as:

```text
releases\v1.0.2\win-x64
releases\v1.0.3\win-x64
...
releases\v1.0.11\win-x64
releases\v1.1.0\win-x64
releases\v1.1.1\win-x64
```

These folders are intentionally ignored because they contain `.exe`, `.dll`, `.pdb`, and other generated output. Rebuild them with:

```bat
build.bat
```

## Notes

- Versions `v1.0.3` through `v1.0.9` existed as local build iterations before the WPF source was committed as a clean checkpoint.
- Do not create fake source tags for versions that do not have matching source commits.
- Future releases should be made as normal commits plus annotated Git tags.
