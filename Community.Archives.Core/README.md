# Community.Archives.Core

A collection of libraries that support fast and efficient forward-only reading of various popular archives. 

This is the `Core` package that contains shared code. It doesn't contain the actual readers but the `IArchiveReader` interface.

* :rocket: Fast and efficient: Only extracts matched file. Forward-only access. Uses `Task` to offload `IO` to separate threads.
* :grinning: Licensed under MIT. Similar projects are licensed under GPL.
* :heart_eyes: 100% test coverage 

## Supported archive formats

| Format | Package                                                                                          | Documentation                                                                                |
| ------ | ------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------- |
| Ar     | [Get `Community.Archives.Ar` on nuget](https://www.nuget.org/packages/Community.Archives.Ar)     | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Ar)   |
| Cpio   | [Get `Community.Archives.Cpio` on nuget](https://www.nuget.org/packages/Community.Archives.Cpio) | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Cpio) |
| Rpm    | [Get `Community.Archives.Rpm` on nuget](https://www.nuget.org/packages/Community.Archives.Rpm)   | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Rpm)  |
| Tar    | [Get `Community.Archives.Tar` on nuget](https://www.nuget.org/packages/Community.Archives.Tar)   | [Get started](https://github.com/Gitii/Community.Archives/tree/main/Community.Archives.Tar)  |
